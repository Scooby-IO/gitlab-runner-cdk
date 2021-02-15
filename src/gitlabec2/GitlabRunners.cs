using Amazon.CDK;
using Amazon.CDK.AWS.AutoScaling;
using Amazon.CDK.AWS.ApplicationAutoScaling;
using Amazon.CDK.AWS.EC2;
using System.Collections.Generic;


namespace CdkTesting
{
    public class GitlabRunners : Stack
    {

        internal GitlabRunners(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {

            IVpc vpc = Vpc.FromLookup(this, "VPC", new VpcLookupOptions
            {
                // This imports the default VPC but you can also
                // specify a 'vpcName' or 'tags'.
                IsDefault = true
            });


            // Security Group Creation
            var InstanceSecurityGroup = new SecurityGroup(this, "SecurityGroupBasicAccess", new SecurityGroupProps
            {
                Vpc = vpc,
                SecurityGroupName = "Gitlab-SG",
                Description = "Security Group for Gitlab Runner Access",
                AllowAllOutbound = true
            });

            // Security Group's Inbound and Outbound rules
            InstanceSecurityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(3389), "Allows public RDP access");

            // Configuring custom CENOTS AMI
            IDictionary<string, string> d = new Dictionary<string, string>();

            d.Add(new KeyValuePair<string, string>(Region, "ami-026f33d38b6410e30"));

            var userDataWindows = UserData.ForWindows();
            userDataWindows.AddCommands(new string[]
            {
                "Set-ExecutionPolicy Bypass -Force;",
                "iex ((new-object net.webclient).DownloadString('https://chocolatey.org/install.ps1'));",
                "choco install python -y",
                "choco install gitlab-runner -y;",
                "c:\\gitlab-runner\\gitlab-runner.exe install;",
                "c:\\gitlab-runner\\gitlab-runner.exe register --non-interactive --url 'https://gitlab.com/' --registration-token 'PyhV_iLfRV5sZ7tJPwrq' --executor 'shell' --tag-list 'scooby,aws'",
                "Start-Service gitlab-runner"
            });

            // Instance Detail Configuration
            var ec2Instance = new Instance_(this, "Instance", new InstanceProps
            {
                Vpc = vpc,
                InstanceType = new InstanceType("t3.small"),
                MachineImage = MachineImage.LatestWindows(WindowsVersion.WINDOWS_SERVER_1809_ENGLISH_CORE_CONTAINERSLATEST),
                SecurityGroup = InstanceSecurityGroup,
                VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE },
                //KeyName = keyPairName.ValueAsString,
                InstanceName = "Gitlab-Runner",
                UserData = userDataWindows,
                UserDataCausesReplacement = true,
                BlockDevices = new Amazon.CDK.AWS.EC2.IBlockDevice[]
                {
                    new Amazon.CDK.AWS.EC2.BlockDevice
                    {
                        DeviceName = "/dev/sda1",
                        Volume = Amazon.CDK.AWS.EC2.BlockDeviceVolume.Ebs(60, new Amazon.CDK.AWS.EC2.EbsDeviceOptions
                        {
                            VolumeType = Amazon.CDK.AWS.EC2.EbsDeviceVolumeType.GP2
                        })
                    }
                }
            });

            /*
            AutoScalingGroup asg = new AutoScalingGroup(this, "test-asg", new AutoScalingGroupProps
            {
                Vpc = vpc,
                DesiredCapacity = 2,
                MaxCapacity = 2,
                MinCapacity = 2,
                MachineImage = MachineImage.LatestWindows(WindowsVersion.WINDOWS_SERVER_1809_ENGLISH_CORE_CONTAINERSLATEST),
                InstanceType = new InstanceType("t3.small"),
                AutoScalingGroupName = "gitlab-runner-asg",
                SecurityGroup = InstanceSecurityGroup,
                VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE },
                UpdatePolicy = UpdatePolicy.RollingUpdate(new RollingUpdateOptions { }),
                UserData = userDataWindows,

            });
            */
        }   
    }
}
