using System;
using System.Collections;
using System.IO;
using System.Threading;

using System.Runtime.InteropServices;
using MonoDevelop.Core;
using MonoDevelop.VersionControl.Subversion.Gui;

namespace MonoDevelop.VersionControl.Subversion {
	public class SvnClient {
		static LibSvnClient svn;
		static LibApr apr;
		IntPtr auth_baton;
		IntPtr pool;
		IntPtr ctx;
		
		object sync = new object();
		bool inProgress = false;
		
		IProgressMonitor updatemonitor;
		string commitmessage = null;
		
		// retain this so the delegates aren't GC'ed
		LibSvnClient.svn_client_ctx_t ctxstruct;
		
		static SvnClient()
		{
			// libsvn_client may be linked to libapr-0 or libapr-1, and we need to bind the LibApr class
			// the the same library. The following code detects the required libapr version and loads it. 
			int aprver = GetLoadAprLib (-1);
			svn = LibSvnClient.GetLib ();
			aprver = GetLoadAprLib (aprver);
			if (aprver != -1)
				Runtime.LoggingService.Info ("Subversion add-in: detected libapr-" + aprver);
			apr = LibApr.GetLib (aprver);
		}
		
		static int GetLoadAprLib (int oldVersion)
		{
			// Get the version of the loaded libapr
			string file = "/proc/" + System.Diagnostics.Process.GetCurrentProcess ().Id + "/maps";
			try {
				int newv = oldVersion;
				if (File.Exists (file)) {
					string txt = File.ReadAllText (file);
					if (txt.IndexOf ("libapr-0") != -1 && oldVersion != 0)
						newv = 0;
					if (txt.IndexOf ("libapr-1") != -1 && oldVersion != 1)
						newv = 1;
				}
				return newv;
			} catch {
				// Ignore
				return oldVersion;
			}
		}
		
		private static IntPtr newpool (IntPtr parent)
		{
			IntPtr p;
			apr.pool_create_ex (out p, parent, IntPtr.Zero, IntPtr.Zero);
			if (p == IntPtr.Zero)
				throw new InvalidOperationException ("Could not create an APR pool.");
			return p;
		}
		
		public SvnClient ()
		{
			// Allocate the APR pool and the SVN client context.
			pool = newpool (IntPtr.Zero);

			// Make sure the config directory is properly created.
			// If the config directory and specifically the subdirectories
			// for the authentication providers don't exist, authentication
			// data won't be saved and no error is given.
			svn.config_ensure (null, pool);
			
			if (svn.client_create_context (out ctx, pool) != IntPtr.Zero)
				throw new InvalidOperationException ("Could not create a Subversion client context.");
			
			// Set the callbacks on the client context structure.
			
			// This is quite a roudabout way of doing this.  The task
			// is to set the notify field of the unmanaged structure
			// at the address 'ctx' -- with the address of a delegate.
			// There's no way to get an address for the delegate directly,
			// as far as I could figure out, so instead I create a managed
			// structure that mirrors the start of the unmanaged structure
			// I want to modify.  Then I marshal the managed structure
			// *onto* to unmanaged one, overwriting fields in the process.
			// I don't use references to the structure itself in the API
			// calls because it needs to be allocated by SVN.  I hope
			// this doesn't cause any memory leaks.
			ctxstruct = new LibSvnClient.svn_client_ctx_t ();
			ctxstruct.NotifyFunc = new LibSvnClient.svn_wc_notify_func_t (svn_wc_notify_func_t_impl);
			ctxstruct.LogMsgFunc = new LibSvnClient.svn_client_get_commit_log_t (svn_client_get_commit_log_impl);
			
			IntPtr providers = apr.array_make (pool, 1, IntPtr.Size);
			IntPtr item;
			
		    // The main disk-caching auth providers, for both
		    // 'username/password' creds and 'username' creds.
			
			item = apr.array_push (providers);
			svn.client_get_simple_provider (item, pool);
			
			item = apr.array_push (providers);
			svn.client_get_username_provider (item, pool);
			
			// The server-cert, client-cert, and client-cert-password providers
			
			item = apr.array_push (providers);
			svn.client_get_ssl_server_trust_file_provider (item, pool);
			
			item = apr.array_push (providers);
			svn.client_get_ssl_client_cert_file_provider (item, pool);
			
			item = apr.array_push (providers);
			svn.client_get_ssl_client_cert_pw_file_provider (item, pool);

			// Two basic prompt providers: username/password, and just username.

			item = apr.array_push (providers);
			svn.client_get_simple_prompt_provider (item, new LibSvnClient.svn_auth_simple_prompt_func_t (OnAuthSimplePrompt), IntPtr.Zero, 2, pool);
			
			item = apr.array_push (providers);
			svn.client_get_username_prompt_provider (item, new LibSvnClient.svn_auth_username_prompt_func_t (OnAuthUsernamePrompt), IntPtr.Zero, 2, pool);
			
			// Three ssl prompt providers, for server-certs, client-certs,
			// and client-cert-passphrases.
			
			item = apr.array_push (providers);
			svn.client_get_ssl_server_trust_prompt_provider (item, new LibSvnClient.svn_auth_ssl_server_trust_prompt_func_t (OnAuthSslServerTrustPrompt), IntPtr.Zero, pool);
			
			item = apr.array_push (providers);
			svn.client_get_ssl_client_cert_prompt_provider (item, new LibSvnClient.svn_auth_ssl_client_cert_prompt_func_t (OnAuthSslClientCertPrompt), IntPtr.Zero, 2, pool);
			
			item = apr.array_push (providers);
			svn.client_get_ssl_client_cert_pw_prompt_provider (item, new LibSvnClient.svn_auth_ssl_client_cert_pw_prompt_func_t (OnAuthSslClientCertPwPrompt), IntPtr.Zero, 2, pool);
			
			// Create the authentication baton			
			
			svn.auth_open (out auth_baton, providers, pool); 
			ctxstruct.auth_baton = auth_baton;
			
			Marshal.StructureToPtr (ctxstruct, ctx, false);
		}
		
		~SvnClient ()
		{
			apr.pool_destroy(pool);
		}
		
		static IntPtr GetCancelError ()
		{
			LibSvnClient.svn_error_t error = new LibSvnClient.svn_error_t ();
			error.apr_err = LibApr.APR_OS_START_USEERR;
			error.message = "Operation cancelled.";
			
			// Subversion destroys the error pool to dispose the error object,
			// so we need to use a non-shared pool.
			IntPtr localpool = newpool (IntPtr.Zero);
			error.pool = localpool;
			return apr.pcalloc (localpool, error);
		}
		
		static IntPtr OnAuthSimplePrompt (ref IntPtr cred, IntPtr baton, [MarshalAs (UnmanagedType.LPStr)] string realm, [MarshalAs (UnmanagedType.LPStr)] string user_name, [MarshalAs (UnmanagedType.SysInt)] int may_save, IntPtr pool)
		{
			LibSvnClient.svn_auth_cred_simple_t data; 
			if (UserPasswordDialog.Show (user_name, realm, may_save != 0, out data)) {
				cred = apr.pcalloc (pool, data);
				return IntPtr.Zero;
			} else {
				data.password = "";
				data.username = "";
				data.may_save = 0;
				cred = apr.pcalloc (pool, data);
				return GetCancelError ();
			}
		}
		
		static IntPtr OnAuthUsernamePrompt (ref IntPtr cred, IntPtr baton, [MarshalAs (UnmanagedType.LPStr)] string realm, [MarshalAs (UnmanagedType.SysInt)] int may_save, IntPtr pool)
		{
			LibSvnClient.svn_auth_cred_username_t data;
			if (UserPasswordDialog.Show (realm, may_save != 0, out data)) {
				cred = apr.pcalloc (pool, data);
				return IntPtr.Zero;
			} else {
				data.username = "";
				data.may_save = 0;
				cred = apr.pcalloc (pool, data);
				return GetCancelError ();
			}
		}
		
		static IntPtr OnAuthSslServerTrustPrompt (ref IntPtr cred, IntPtr baton, [MarshalAs (UnmanagedType.LPStr)] string realm, uint failures, ref LibSvnClient.svn_auth_ssl_server_cert_info_t cert_info, [MarshalAs (UnmanagedType.SysInt)] int may_save, IntPtr pool)
		{
			LibSvnClient.svn_auth_cred_ssl_server_trust_t data;
			if (SslServerTrustDialog.Show (realm, failures, may_save, cert_info, out data) && data.accepted_failures != 0) {
				cred = apr.pcalloc (pool, data);
				return IntPtr.Zero;
			} else {
				data.accepted_failures = 0;
				data.may_save = 0;
				cred = apr.pcalloc (pool, data);
				return GetCancelError ();
			}
		}
		
		static IntPtr OnAuthSslClientCertPrompt (ref IntPtr cred, IntPtr baton, [MarshalAs (UnmanagedType.LPStr)] string realm, [MarshalAs (UnmanagedType.SysInt)] int may_save, IntPtr pool)
		{
			LibSvnClient.svn_auth_cred_ssl_client_cert_t data;
			if (ClientCertificateDialog.Show (realm, may_save, out data)) {
				cred = apr.pcalloc (pool, data);
				return IntPtr.Zero;
			} else {
				data.cert_file = "";
				data.may_save = 0;
				cred = apr.pcalloc (pool, data);
				return GetCancelError ();
			}
		}
		
		static IntPtr OnAuthSslClientCertPwPrompt (ref IntPtr cred, IntPtr baton, [MarshalAs (UnmanagedType.LPStr)] string realm, [MarshalAs (UnmanagedType.SysInt)] int may_save, IntPtr pool)
		{
			LibSvnClient.svn_auth_cred_ssl_client_cert_pw_t data;
			if (ClientCertificatePasswordDialog.Show (realm, may_save, out data)) {
				cred = apr.pcalloc (pool, data);
				return IntPtr.Zero;
			} else {
				data.password = "";
				data.may_save = 0;
				cred = apr.pcalloc (pool, data);
				return GetCancelError ();
			}
		}
		
		// Wrappers for native interop
		
		public string Version {
			get {
				IntPtr ptr = svn.client_version ();
				LibSvnClient.svn_version_t ver = (LibSvnClient.svn_version_t) Marshal.PtrToStructure (ptr, typeof (LibSvnClient.svn_version_t));				
				return ver.major + "." + ver.minor + "." + ver.patch;
			}
		}
		
		public IList List (string pathorurl, bool recurse, LibSvnClient.Rev revision)
		{
			if (pathorurl == null)
				throw new ArgumentNullException ();
			
			IntPtr localpool = newpool (pool);
			ArrayList items = new ArrayList ();
			try {
				IntPtr hash;
				
				CheckError (svn.client_ls (out hash, pathorurl, ref revision,
				                           recurse ? 1 : 0, ctx, localpool));
				
				IntPtr item = apr.hash_first (localpool, hash);
				while (item != IntPtr.Zero) {
					IntPtr nameptr, val;
					int namelen;
					apr.hash_this (item, out nameptr, out namelen, out val);
					
					string name = Marshal.PtrToStringAnsi (nameptr);
					LibSvnClient.svn_dirent_t ent = (LibSvnClient.svn_dirent_t) Marshal.PtrToStructure (val, typeof (LibSvnClient.svn_dirent_t));				
					item = apr.hash_next (item);
					
					items.Add (new LibSvnClient.DirEnt (name, ent));
				}
			} finally {
				apr.pool_destroy (localpool);
			}
			
			return items;
		}

		public IList Status (string path, LibSvnClient.Rev revision)
		{
			return Status (path, revision, false, false, false);
		}
		
		public IList Status (string path, LibSvnClient.Rev revision, bool descendDirs, bool changedItemsOnly, bool remoteStatus)
		{
			if (path == null)
				throw new ArgumentNullException ();
			
			ArrayList ret = new ArrayList ();
			
			StatusCollector collector = new StatusCollector (ret);
			
			IntPtr localpool = newpool (pool);
			try {
				CheckError (svn.client_status (IntPtr.Zero, path, ref revision,
				                               new LibSvnClient.svn_wc_status_func_t (collector.Func),
				                               IntPtr.Zero, descendDirs ? 1 : 0, 
				                               changedItemsOnly ? 0 : 1, 
				                               remoteStatus ? 1 : 0, 1,
				                               ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
			}
			
			return ret;
		}
		
		public IList Log (string path, LibSvnClient.Rev revisionStart, LibSvnClient.Rev revisionEnd)
		{
			if (path == null)
				throw new ArgumentNullException ();
			
			ArrayList ret = new ArrayList ();
			IntPtr localpool = newpool (pool);
			IntPtr strptr = IntPtr.Zero;
			try {
				IntPtr array = apr.array_make (localpool, 0, IntPtr.Size);
				IntPtr first = apr.array_push (array);
				strptr = Marshal.StringToHGlobalAnsi (path);
				Marshal.WriteIntPtr (first, strptr);
				
				LogCollector collector = new LogCollector (ret);
				
				CheckError (svn.client_log (array, ref revisionStart, ref revisionEnd, 1, 0,
				                            new LibSvnClient.svn_log_message_receiver_t (collector.Func),
				                            IntPtr.Zero, ctx, localpool));
			} finally {
				if (strptr != IntPtr.Zero)
					Marshal.FreeHGlobal (strptr);
				apr.pool_destroy (localpool);
			}
			
			return ret;
		}
		
		public string GetPathUrl (string path)
		{
			if (path == null)
				throw new ArgumentNullException();
			
			IntPtr ret = IntPtr.Zero;
			IntPtr localpool = newpool (pool);
			try {
				CheckError (svn.client_url_from_path (ref ret, path, localpool));
			} finally {
				apr.pool_destroy (localpool);
			}
			
			if (ret == IntPtr.Zero)
				return null;
			
			return Marshal.PtrToStringAnsi (ret);
		}
		
		public string Cat (string pathorurl, LibSvnClient.Rev revision)
		{
			MemoryStream memstream = new MemoryStream ();
			Cat (pathorurl, revision, memstream);
			
			try {
				return System.Text.Encoding.UTF8.GetString (memstream.GetBuffer ());
			} catch {
			}
			
			return System.Text.Encoding.ASCII.GetString (memstream.GetBuffer ());
		}
		
		public void Cat (string pathorurl, LibSvnClient.Rev revision, Stream stream)
		{
			if (pathorurl == null || stream == null)
				throw new ArgumentNullException ();
			
			IntPtr localpool = newpool (pool);
			try {
				StreamCollector collector = new StreamCollector (stream);
				IntPtr svnstream = svn.stream_create (IntPtr.Zero, localpool);
				svn.stream_set_write (svnstream, new LibSvnClient.svn_readwrite_fn_t (collector.Func));
				LibSvnClient.Rev peg_revision = LibSvnClient.Rev.Blank;
				CheckError (svn.client_cat2 (svnstream, pathorurl, ref peg_revision, ref revision, ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
			}
		}
		
		public void Update (string path, bool recurse, IProgressMonitor monitor)
		{
			if (path == null || monitor == null)
				throw new ArgumentNullException();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			
			LibSvnClient.Rev rev = LibSvnClient.Rev.Head;
			IntPtr localpool = newpool (pool);
			try {
				CheckError (svn.client_update (IntPtr.Zero, path, ref rev, recurse ? 1 : 0, ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
				updatemonitor = null;
				inProgress = false;
			}
		}
		
		public void Revert (string[] paths, bool recurse, IProgressMonitor monitor)
		{
			if (paths == null || monitor == null)
				throw new ArgumentNullException();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			IntPtr localpool = newpool (pool);
			
			// Put each item into an APR array.
			IntPtr array = apr.array_make (localpool, 0, IntPtr.Size);
			foreach (string path in paths) {
				IntPtr item = apr.array_push (array);
				Marshal.WriteIntPtr (item, apr.pstrdup (localpool, path));
			}
			
			try {
				CheckError (svn.client_revert (array, recurse ? 1 : 0, ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
				updatemonitor = null;
				inProgress = false;
			}
		}
		
		public void Add (string path, bool recurse, IProgressMonitor monitor)
		{
			if (path == null || monitor == null)
				throw new ArgumentNullException ();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException ("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			IntPtr localpool = newpool (pool);
			try {
				CheckError (svn.client_add3 (path, (recurse ? 1 : 0), 1, 0, ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
				updatemonitor = null;
				inProgress = false;
			}
		}
		
		public void Checkout (string url, string path, LibSvnClient.Rev rev, bool recurse, IProgressMonitor monitor)
		{
			if (url == null || monitor == null)
				throw new ArgumentNullException ();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException ("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			IntPtr result_rev = IntPtr.Zero;
			IntPtr localpool = newpool (pool);
			try {
				CheckError (svn.client_checkout (result_rev, url, path, ref rev, (recurse ? 1 :0), ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
				updatemonitor = null;
				inProgress = false;
			}
		}
		
		public void Commit (string[] paths, string message, IProgressMonitor monitor)
		{
			if (paths == null || message == null || monitor == null)
				throw new ArgumentNullException();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException ("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			
			IntPtr localpool = newpool (pool);
			try {
				// Put each item into an APR array.
				IntPtr array = apr.array_make (localpool, 0, IntPtr.Size);
				foreach (string path in paths) {
					IntPtr item = apr.array_push (array);
					Marshal.WriteIntPtr (item, apr.pstrdup (localpool, path));
				}
				
				IntPtr commit_info = IntPtr.Zero;
				
				commitmessage = message;
				
				CheckError (svn.client_commit (ref commit_info, array, 0, ctx, localpool));
                                unsafe {
                                        monitor.Log.WriteLine ("Revision: " + ((LibSvnClient.svn_client_commit_info_t *) commit_info.ToPointer())->revision);
                                }
			} finally {
				commitmessage = null;
				updatemonitor = null;
				apr.pool_destroy (localpool);
				inProgress = false;
			}
		}
		
		public void Mkdir (string[] paths, string message, IProgressMonitor monitor) 
		{
			if (paths == null || monitor == null)
				throw new ArgumentNullException ();
		
			lock (sync) {
				if (inProgress)
					throw new SubversionException ("Another Subversion operation is already in progress.");
				inProgress = true;
			}

			updatemonitor = monitor;
			
			IntPtr localpool = newpool(pool);
			try {
				// Put each item into an APR array.
				IntPtr array = apr.array_make (localpool, paths.Length, IntPtr.Size);
				foreach (string path in paths) {
					IntPtr item = apr.array_push (array);
					Marshal.WriteIntPtr (item, apr.pstrdup (localpool, path));
				}
				
				commitmessage = message;

				IntPtr commit_info = IntPtr.Zero;
				CheckError (svn.client_mkdir2 (ref commit_info, array, ctx, localpool));
			} finally {	
				commitmessage = null;
				updatemonitor = null;
				apr.pool_destroy (localpool);
				inProgress = false;
			}
		}
		
		public void Delete (string path, bool force, IProgressMonitor monitor)
		{
			if (path == null || monitor == null)
				throw new ArgumentNullException ();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException ("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			
			IntPtr localpool = newpool (pool);
			try {
				// Put each item into an APR array.
				IntPtr array = apr.array_make (localpool, 0, IntPtr.Size);
				//foreach (string path in paths) {
					IntPtr item = apr.array_push (array);
					Marshal.WriteIntPtr (item, apr.pstrdup (localpool, path));
				//}
				IntPtr commit_info = IntPtr.Zero;
				CheckError (svn.client_delete (ref commit_info, array, (force ? 1 : 0), ctx, localpool));
			} finally {
				commitmessage = null;
				updatemonitor = null;
				apr.pool_destroy (localpool);
				inProgress = false;
			}
		}
		
		public void Move (string srcPath, string destPath, LibSvnClient.Rev revision, bool force, IProgressMonitor monitor)
		{
			if (srcPath == null || destPath == null || monitor == null)
				throw new ArgumentNullException ();
			
			lock (sync) {
				if (inProgress)
					throw new SubversionException ("Another Subversion operation is already in progress.");
				inProgress = true;
			}
			
			updatemonitor = monitor;
			IntPtr commit_info = IntPtr.Zero;
			IntPtr localpool = newpool (pool);
			try {
				CheckError (svn.client_move (ref commit_info, srcPath, ref revision,
				                             destPath, (force ? 1 : 0), ctx, localpool));
			} finally {
				apr.pool_destroy (localpool);
				updatemonitor = null;
				inProgress = false;
			}
		}
		
		public string PathDiff (string path1, LibSvnClient.Rev revision1, string path2, LibSvnClient.Rev revision2, bool recursive)
		{
			IntPtr localpool = newpool (pool);
			IntPtr outfile = IntPtr.Zero;
			IntPtr errfile = IntPtr.Zero;
			string fout = null;
			string ferr = null;
			
			try {
				IntPtr options = apr.array_make (localpool, 0, IntPtr.Size);
				
				fout = Path.GetTempFileName ();
				ferr = Path.GetTempFileName ();
				int res1 = apr.file_open (ref outfile, fout, LibApr.APR_WRITE | LibApr.APR_CREATE | LibApr.APR_TRUNCATE, LibApr.APR_OS_DEFAULT, localpool);
				int res2 = apr.file_open (ref errfile, ferr, LibApr.APR_WRITE | LibApr.APR_CREATE | LibApr.APR_TRUNCATE, LibApr.APR_OS_DEFAULT, localpool);
				
				if (res1 == 0 && res2 == 0) {
					CheckError (svn.client_diff (options, path1, ref revision1, path2, ref revision2, (recursive ? 1 : 0), 0, 1, outfile, errfile, ctx, localpool));
					return fout;
				} else {
					throw new Exception ("Could not get diff information");
				}
			} catch {
				try {
					if (outfile != IntPtr.Zero)
						apr.file_close (outfile);
					if (fout != null)
						FileService.DeleteFile (fout);
					outfile = IntPtr.Zero;
					fout = null;
				} catch {}
				throw;
			} finally {
				try {
					// Cleanup
					apr.pool_destroy (localpool);
					if (outfile != IntPtr.Zero)
						apr.file_close (outfile); 
					if (errfile != IntPtr.Zero)
						apr.file_close (errfile);
					if (ferr != null)
						FileService.DeleteFile (ferr);
				} catch {}
			}
		}
		
		IntPtr svn_client_get_commit_log_impl (ref IntPtr log_msg, ref IntPtr tmp_file,
		                                       IntPtr commit_items, IntPtr baton, IntPtr pool)
		{
			log_msg = apr.pstrdup (pool, commitmessage);
			tmp_file = IntPtr.Zero;
			return IntPtr.Zero;
		}
		
		private void CheckError (IntPtr error)
		{
			if (error == IntPtr.Zero)
				return;
			
			string msg = null;
			while (error != IntPtr.Zero) {
				LibSvnClient.svn_error_t error_t = (LibSvnClient.svn_error_t) Marshal.PtrToStructure (error, typeof (LibSvnClient.svn_error_t));
				if (msg != null)
					msg += "\n" + error_t.message;
				else
					msg = error_t.message;
				error = error_t.svn_error_t_child;
			}
			
			if (msg == null)
				msg = "Unknown error";
			
			throw new SubversionException (msg);
		}
		
		void svn_wc_notify_func_t_impl (IntPtr baton, IntPtr path, LibSvnClient.NotifyAction action,
		                                LibSvnClient.NodeKind kind, IntPtr mime_type,
		                                LibSvnClient.NotifyState content_state,
		                                LibSvnClient.NotifyState prop_state,
		                                long revision)
		{
			string actiondesc = action.ToString ();
			switch (action) {
			case LibSvnClient.NotifyAction.UpdateAdd: actiondesc = "Added"; break;
			case LibSvnClient.NotifyAction.UpdateDelete: actiondesc = "Deleted"; break;
			case LibSvnClient.NotifyAction.UpdateUpdate: actiondesc = "Updating"; break;
			case LibSvnClient.NotifyAction.UpdateExternal: actiondesc = "External Updated"; break;
			case LibSvnClient.NotifyAction.UpdateCompleted: actiondesc = "Finished"; break;
				
			case LibSvnClient.NotifyAction.CommitAdded: actiondesc = "Added"; break;
			case LibSvnClient.NotifyAction.CommitDeleted: actiondesc = "Deleted"; break;
			case LibSvnClient.NotifyAction.CommitModified: actiondesc = "Modified"; break;
			case LibSvnClient.NotifyAction.CommitReplaced: actiondesc = "Replaced"; break;
			case LibSvnClient.NotifyAction.CommitPostfixTxDelta: actiondesc = "Sending Content"; break;
				/*Add,
				 Copy,
				 Delete,
				 Restore,
				 Revert,
				 FailedRevert,
				 Resolved,
				 Skip,
				 StatusCompleted,
				 StatusExternal,
				 BlameRevision*/
			}
			
			if (updatemonitor != null)
				updatemonitor.Log.WriteLine (actiondesc + " " + Marshal.PtrToStringAnsi (path));
		}
		
		public class StatusCollector {
			ArrayList statuses;
			
			public StatusCollector (ArrayList statuses) { this.statuses = statuses; }
			
			public void Func (IntPtr baton, IntPtr path, ref LibSvnClient.svn_wc_status_t status)
			{
				string pathstr = Marshal.PtrToStringAnsi (path);
				/*if (status.to_svn_wc_entry_t == IntPtr.Zero)
					return;
				 */
				statuses.Add (new LibSvnClient.StatusEnt (status, pathstr));
			}
		}
		
		private class LogCollector {
			static readonly DateTime Epoch = new DateTime (1970, 1, 1);
			
			ArrayList logs;
			
			public LogCollector (ArrayList logs) { this.logs = logs; }
			
			public IntPtr Func (IntPtr baton, IntPtr apr_hash_changed_paths, int revision, IntPtr author, IntPtr date, IntPtr message, IntPtr pool)
			{
				long time;
				svn.time_from_cstring (out time, Marshal.PtrToStringAnsi (date), pool);
				string smessage = "";
				
				if (message != IntPtr.Zero)
					smessage = Marshal.PtrToStringAnsi (message).Trim ();
				
				ArrayList items = new ArrayList();
				
				IntPtr item = apr.hash_first (pool, apr_hash_changed_paths);
				while (item != IntPtr.Zero) {
					IntPtr nameptr, val;
					int namelen;
					apr.hash_this (item, out nameptr, out namelen, out val);
					
					string name = Marshal.PtrToStringAnsi (nameptr);
					LibSvnClient.svn_log_changed_path_t ch = (LibSvnClient.svn_log_changed_path_t) Marshal.PtrToStructure (val, typeof (LibSvnClient.svn_log_changed_path_t));
					item = apr.hash_next (item);
					
					items.Add (new LibSvnClient.LogEntChangedPath (name, ch));
				}
				
				logs.Add (new LibSvnClient.LogEnt (revision, Marshal.PtrToStringAnsi (author), Epoch.AddTicks (time * 10),
				                                   smessage, (LibSvnClient.LogEntChangedPath[]) items.ToArray (typeof (LibSvnClient.LogEntChangedPath))));
				
				return IntPtr.Zero;
			}
		}
		
		public class StreamCollector {
			Stream buf;
			
			public StreamCollector (Stream buf) { this.buf = buf; }
			
			public IntPtr Func (IntPtr baton, IntPtr data, ref IntPtr len)
			{
				unsafe {
					byte *bp = (byte *) data;
					int max = (int) len;
					for (int i = 0; i < max; i++) {
						buf.WriteByte (*bp);
						bp++;
					}
				}
				return IntPtr.Zero;
			}
		}
	}
}
