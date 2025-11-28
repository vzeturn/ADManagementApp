using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Secure credential storage using Windows Credential Manager
    /// </summary>
    public class CredentialService : ICredentialService
    {
        private const string CredentialTarget = "ADManagementApp_Credentials";
        private readonly ILogger<CredentialService> _logger;

        public CredentialService(ILogger<CredentialService> logger)
        {
            _logger = logger;
        }

        public Task SaveCredentialsAsync(string domain, string username, string password)
        {
            return Task.Run(() =>
            {
                try
                {
                    var credential = $"{domain}|{username}|{password}";
                    WriteCredential(CredentialTarget, credential);
                    _logger.LogInformation("Credentials saved securely for domain: {Domain}", domain);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save credentials");
                    throw;
                }
            });
        }

        public Task<(string Domain, string Username, string Password)?> GetCredentialsAsync()
        {
            return Task.Run<(string, string, string)?>(() =>
            {
                try
                {
                    var credential = ReadCredential(CredentialTarget);
                    if (string.IsNullOrEmpty(credential))
                    {
                        _logger.LogDebug("No stored credentials found");
                        return null;
                    }

                    var parts = credential.Split('|');
                    if (parts.Length != 3)
                    {
                        _logger.LogWarning("Invalid credential format");
                        return null;
                    }

                    return (parts[0], parts[1], parts[2]);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve credentials");
                    return null;
                }
            });
        }

        public Task DeleteCredentialsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    DeleteCredential(CredentialTarget);
                    _logger.LogInformation("Credentials deleted");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete credentials");
                    throw;
                }
            });
        }

        public Task<bool> HasStoredCredentialsAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    var credential = ReadCredential(CredentialTarget);
                    return !string.IsNullOrEmpty(credential);
                }
                catch
                {
                    return false;
                }
            });
        }

        #region Windows Credential Manager P/Invoke

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public int Flags;
            public int Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredWriteW", CharSet = CharSet.Unicode)]
        private static extern bool CredWrite([In] ref CREDENTIAL userCredential, [In] int flags);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredReadW", CharSet = CharSet.Unicode)]
        private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("Advapi32.dll", SetLastError = true, EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        private static extern bool CredDelete(string target, int type, int reservedFlag);

        [DllImport("Advapi32.dll", SetLastError = true)]
        private static extern bool CredFree([In] IntPtr cred);

        private void WriteCredential(string target, string credential)
        {
            var byteArray = Encoding.Unicode.GetBytes(credential);
            var credentialBlob = Marshal.AllocHGlobal(byteArray.Length);
            Marshal.Copy(byteArray, 0, credentialBlob, byteArray.Length);

            var cred = new CREDENTIAL
            {
                Type = 1, // CRED_TYPE_GENERIC
                TargetName = target,
                CredentialBlob = credentialBlob,
                CredentialBlobSize = byteArray.Length,
                Persist = 2, // CRED_PERSIST_LOCAL_MACHINE
                UserName = Environment.UserName
            };

            try
            {
                if (!CredWrite(ref cred, 0))
                {
                    throw new InvalidOperationException($"Failed to write credential. Error: {Marshal.GetLastWin32Error()}");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(credentialBlob);
            }
        }

        private string? ReadCredential(string target)
        {
            if (!CredRead(target, 1, 0, out IntPtr credentialPtr))
            {
                return null;
            }

            try
            {
                var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
                var credentialBlob = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, credentialBlob, 0, credential.CredentialBlobSize);
                return Encoding.Unicode.GetString(credentialBlob);
            }
            finally
            {
                CredFree(credentialPtr);
            }
        }

        private void DeleteCredential(string target)
        {
            if (!CredDelete(target, 1, 0))
            {
                var error = Marshal.GetLastWin32Error();
                if (error != 1168) // ERROR_NOT_FOUND
                {
                    throw new InvalidOperationException($"Failed to delete credential. Error: {error}");
                }
            }
        }

        #endregion
    }
}