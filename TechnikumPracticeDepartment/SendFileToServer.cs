using Renci.SshNet;
using Renci.SshNet.Common;
using ConnectionInfo = Renci.SshNet.ConnectionInfo;

namespace TechnikumPracticeDepartment
{
    public class SendFileToServer
    {
        private IConfiguration configuration;
        public SendFileToServer(IConfiguration conf)
        {
            configuration = conf;
        }
        public int SendFileImageStudent(IFormFile uploadedFile, string id, string extension)
        {
            var connectionInfo = new ConnectionInfo(configuration["TechnikumPracticeDepartment:HostIP_FTP"], "root", new PasswordAuthenticationMethod(configuration["TechnikumPracticeDepartment:UserName_FTP"], configuration["TechnikumPracticeDepartment:PasswordName_FTP"]));
            using (var sftp = new SftpClient(connectionInfo))
            {
                sftp.Connect();
                try
                {
                    sftp.ChangeDirectory("/var/www/ElStudent/Students/" + id);
                }
                catch (SftpPathNotFoundException)
                {
                    sftp.CreateDirectory("/var/www/ElStudent/Students/" + id);
                    sftp.ChangeDirectory("/var/www/ElStudent/Students/" + id);
                }
                sftp.UploadFile(uploadedFile.OpenReadStream(), id + extension, true);
                sftp.Disconnect();
            }
            return 0;
        }
        public int DeleteOldFile(string nameFile, int type)
        {
            try
            {
                var connectionInfo = new ConnectionInfo(configuration["TechnikumPracticeDepartment:HostIP_FTP"], "root", new PasswordAuthenticationMethod(configuration["TechnikumPracticeDepartment:UserName_FTP"], configuration["TechnikumPracticeDepartment:PasswordName_FTP"]));
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    if (type == 1)
                        sftp.ChangeDirectory("/var/www/ElStudent/Students");
                    sftp.DeleteFile(nameFile);
                    sftp.Disconnect();
                }
                return 0;
            }
            catch { }
            return 0;
        }
    }
}
