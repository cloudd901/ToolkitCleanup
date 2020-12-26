using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace PCAFFINITY
{
    //https://stackoverflow.com/questions/295538/how-to-provide-user-name-and-password-when-connecting-to-a-network-share/1197430#1197430
    //Pavel Kovalev

    public static class ImpersonationWrapper
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public static void ImpersonateAction(string domainName, string userName, string userPassword, Action actionToExecute)
        {
            try
            {
                const int LOGON32_PROVIDER_DEFAULT = 0;
                const int LOGON32_LOGON_INTERACTIVE = 2;
                bool returnValue = LogonUser(userName, domainName, userPassword,
                    LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out SafeTokenHandle safeTokenHandle);

                if (!returnValue)
                {
                    int ret = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(ret);
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

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);
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
            bool logonSuccessfull = LogonUser(_username, _domain, _password, LogonType.NewCredentials, LogonProvider.WinNT50, ref _token);
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
                CloseHandle(_token);
            }

            _context = null;
        }

        [DllImport("kernel32.dll")]
        private extern static bool CloseHandle(IntPtr handle);

        [DllImport("advapi32.dll", EntryPoint = "LogonUserW", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain,
        string lpszPassword, LogonType dwLogonType, LogonProvider dwLogonProvider, ref IntPtr phToken);
    }

    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}