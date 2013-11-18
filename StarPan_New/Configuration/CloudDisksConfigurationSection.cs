using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace StarPan.Configuration
{
    public class CloudDisksConfigurationSection:ConfigurationSection
    {
        public CloudDisksConfigurationSection()
        {
            
        }

        [ConfigurationProperty("cloudDiskGroup", IsDefaultCollection = true, Options = ConfigurationPropertyOptions.None)]
        public CloudDiskConfigurationCollection CloudDiskGroup
        {
            get { return (CloudDiskConfigurationCollection)base["cloudDiskGroup"]; }
        }
    }
}
