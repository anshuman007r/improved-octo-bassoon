using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp_for_deployment.Data
{
    public class ProjectConstant
    {
        public const string dir = @"C:\Users\user\Documents\images";
        // if you want to change the dir then u have to also make changes in 'applicationhost.xml'
        // in  <virtualDirectory path="directory which you have specified here"> which will be 
        //present in <site name=Credential_user></site>
        //which will be present in C:\Users\user\source\repos\Credential_user\.vs\Credential_user\config
        //Note:-> you have to enable show hidden folder in file explorer options present in control panel 
    }
}
