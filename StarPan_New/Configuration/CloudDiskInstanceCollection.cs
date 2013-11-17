using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace StarPan.Configuration
{
    public class CloudDiskInstanceCollection : ConfigurationElementCollection
    {
        public CloudDiskInstanceCollection()
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
            return new CloudDiskInstance();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CloudDiskInstance)element).Name;
        }

        public CloudDiskInstance this[int i]
        {
            get
            {
                return (CloudDiskInstance)base.BaseGet(i);
            }
        }
        public CloudDiskInstance this[object key]
        {
            get
            {
                return (CloudDiskInstance)base.BaseGet(key);
            }
        }

        public override bool IsReadOnly()
        {
            return base.IsReadOnly();
        }

    }


    public class CloudDiskInstance :ConfigurationElement
    {
        public CloudDiskInstance()
        {
            
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get
            {
                return (string)base["name"];
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
                return (string)base["className"];
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
                return (string)base["dllName"];
            }
            set
            {
                base["dllName"] = value;
            }

        }

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


        [ConfigurationProperty("accessKey")]
        public string AccessKey
        {
            get
            {
                return (string)base["accessKey"];
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
                return (string)base["accessSecret"];
            }
            set
            {
                base["accessSecret"] = value;
            }
        }

    }

}
