using System.Configuration;

namespace StarPan.Configuration
{
    public class CloudDisksConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("cloudDiskGroup", IsDefaultCollection = true, Options = ConfigurationPropertyOptions.None
            )]
        public CloudDiskConfigurationCollection CloudDiskGroup
        {
            get { return (CloudDiskConfigurationCollection) base["cloudDiskGroup"]; }
        }
    }
}