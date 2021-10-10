using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

//https://stackoverflow.com/questions/295538/how-to-provide-user-name-and-password-when-connecting-to-a-network-share/1197430#1197430
//Pavel Kovalev

namespace ToolkitCleanup
{
    public static class ImpersonationWrapper
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void ImpersonateAction(string domainName, string userName, string userPassword, Action actionToExecute)
        {
            try
            {
                const int LOGON32_PROVIDER_DEFAULT = 0;
                const int LOGON32_LOGON_INTERACTIVE = 2;
                bool returnValue = NativeMethods.LogonUser(userName, domainName, userPassword,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out SafeTokenHandle safeTokenHandle);

                if (!returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new Win32Exception(ret);
                }

                using (safeTokenHandle)
                {
                    using WindowsIdentity newId = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                    using WindowsImpersonationContext impersonatedUser = newId.Impersonate();
                    actionToExecute();
                }
            }
            catch
            {
                throw;
            }
        }
    }

    public sealed class ImpersonationContext
    {
        private readonly string _domain, _password, _username;

        private WindowsImpersonationContext _context;

        private IntPtr _token;

        public ImpersonationContext(string domain, string username, string password)
        {
            _domain = string.IsNullOrEmpty(domain) ? "." : domain;
            _username = username;
            _password = password;
        }

        public enum LogonProvider
        {
            Default = 0,  // LOGON32_PROVIDER_DEFAULT
            WinNT35 = 1,
            WinNT40 = 2,  // Use the NTLM logon provider.
            WinNT50 = 3   // Use the negotiate logon provider.
        }

        public enum LogonType : int
        {
            Interactive = 2,
            Network = 3,
            Batch = 4,
            Service = 5,
            Unlock = 7,
            NetworkClearText = 8,
            NewCredentials = 9
        }

        private bool IsInContext
        {
            get { return _context != null; }
        }

        // Changes the Windows identity of this thread. Make sure to always call Leave() at the end.
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Enter()
        {
            if (IsInContext)
            {
                return;
            }

            _token = IntPtr.Zero;
            bool logonSuccessfull = NativeMethods.LogonUser(_username, _domain, _password, LogonType.NewCredentials, LogonProvider.WinNT50, ref _token);
            if (!logonSuccessfull)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            WindowsIdentity identity = new WindowsIdentity(_token);
            _context = identity.Impersonate();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void Leave()
        {
            if (!IsInContext)
            {
                return;
            }

            _context.Undo();

            if (_token != IntPtr.Zero)
            {
                NativeMethods.CloseHandle(_token);
            }

            _context = null;
        }
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseHandle(handle);
        }
    }
}