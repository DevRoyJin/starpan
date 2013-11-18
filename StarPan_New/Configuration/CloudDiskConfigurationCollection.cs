using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace StarPan.Configuration
{
    public class CloudDiskConfigurationCollection : ConfigurationElementCollection
    {
        public CloudDiskConfigurationCollection()
        {

        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }


        protected override string ElementName
        {
            get
            {
                return "cloudDisk";
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CloudDiskConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CloudDiskConfiguration) element).Name;
        }

        public CloudDiskConfiguration this[int i]
        {
            get
            {
                return (CloudDiskConfiguration) base.BaseGet(i);
            }
        }

        public CloudDiskConfiguration this[object key]
        {
            get
            {
                return (CloudDiskConfiguration) base.BaseGet(key);
            }
        }

    }


    public class CloudDiskConfiguration : ConfigurationElement
    {
        public CloudDiskConfiguration()
        {

        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                return (string) base["name"];
            }
            set
            {
                base["name"] = value;
            }

        }

        [ConfigurationProperty("className")]
        public string ClassName
        {
            get
            {
                return (string) base["className"];
            }
            set
            {
                base["className"] = value;
            }

        }

        [ConfigurationProperty("dllName")]
        public string DllName
        {
            get
            {
                return (string) base["dllName"];
            }
            set
            {
                base["dllName"] = value;
            }

        }

        [ConfigurationProperty("root")]
        public string Root
        {
            get
            {
                return (string)base["root"];
            }
            set
            {
                base["root"] = value;
            }

        }


        #region OSS
        [ConfigurationProperty("accessKey")]
        public string AccessKey
        {
            get
            {
                return (string) base["accessKey"];
            }
            set
            {
                base["accessKey"] = value;
            }
        }

        [ConfigurationProperty("accessSecret")]
        public string AccessSerect
        {
            get
            {
                return (string) base["accessSecret"];
            }
            set
            {
                base["accessSecret"] = value;
            }
        }
        #endregion

        #region OAuth1.0

        [ConfigurationProperty("consumerKey")]
        public string ConsumerKey
        {
            get
            {
                return (string) base["consumerKey"];
            }
            set
            {
                base["consumerKey"] = value;
            }
        }

        [ConfigurationProperty("consumerSecret")]
        public string ConsumerSecret
        {
            get
            {
                return (string) base["consumerSecret"];
            }
            set
            {
                base["consumerSecret"] = value;
            }
        }

        [ConfigurationProperty("token")]
        public string Token
        {
            get
            {
                return (string) base["token"];
            }
            set
            {
                base["token"] = value;
            }
        }

        [ConfigurationProperty("tokenSecret")]
        public string TokenSecret
        {
            get
            {
                return (string) base["tokenSecret"];
            }
            set
            {
                base["tokenSecret"] = value;
            }
        }

        #endregion

        #region OAuth2.0
        [ConfigurationProperty("accessToken")]
        public string AccessToken
        {
            get
            {
                return (string)base["accessToken"];
            }
            set
            {
                base["accessToken"] = value;
            }
        }
        #endregion

    }

}
