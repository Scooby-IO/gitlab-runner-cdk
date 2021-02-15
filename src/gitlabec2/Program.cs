using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CdkTesting
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new GitlabRunners(app, "GitlabRunnerStack", new StackProps { 
            Env = new Amazon.CDK.Environment
            {
                //Account = "123456789",
                Region = "us-east-1"
            }
            
            });
            app.Synth();
        }
    }
}
