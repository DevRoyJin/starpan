/*
 * ========================================================================
 * Copyright(c) 2007-2012 情留メ蚊子, All Rights Reserved.
 * Welcom use the KuaiPanSDK.
 * See more information,Please goto http://www.94qing.com
 * ========================================================================
 * 
 * 作 者：情留メ蚊子
 * Q  Q: 540644769
 * 邮 箱：qlwz@qq.com
 * 网 址：http://www.94qing.com
 * ========================================================================
*/
using System;
using System.Collections.Generic;
using PHPLibrary;

namespace KuaiPanSDK.Model
{
    public class MetaData
    {
        public MetaData(PHPArray data)
        {
            this.Data = data;
            this.SetProperty();
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        private void SetProperty()
        {
            this.Path = this.Data["path"];
            this.Root = this.Data["root"];
            this.Hash = this.Data["hash"];
            this.FileId = this.Data["file_id"];
            this.Type = (string)this.Data["type"] == "file" ? TypeEnum.File : (string)this.Data["type"] == "folder" ? TypeEnum.Folder : TypeEnum.Root;
            this.Size = this.Data["size"];
            this.Createtime = this.Data["create_time"];
            this.ModifyTime = this.Data["modify_time"];
            this.Name = this.Data["name"];
            this.Rev = this.Data["rev"];
            this.IsDeleted = this.Data["is_deleted"];
            //this.Files = this.Data["files"];

            this.Files = new List<FileData>();
            if (this.Data["files"].Value != null)
            {
                foreach (KeyValuePair<object, object> k in (PHPArray)this.Data["files"])
                {
                    this.Files.Add(new FileData((PHPArray)k.Value));
                }
                this.Files.Sort(delegate(FileData x, FileData y)
                {
                    if (x.Type == y.Type)
                    {
                        return string.Compare(x.Name, y.Name);
                    }
                    else
                    {
                        return string.Compare(y.Type.ToString(), x.Type.ToString());
                    }
                });
            }
        }

        /// <summary>
        /// 数据
        /// </summary>
        public PHPArray Data { get; private set; }

        /// <summary>
        /// 文件或文件夹相对path的路径
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// kuaipan 或 app_folder
        /// </summary>
        public string Root { get; private set; }

        /// <summary>
        /// list=true才返回,当前这级文件夹的哈希值。
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// path=/,root=kuaipan时不返回。文件唯一标识id。
        /// </summary>
        public string FileId { get; private set; }

        /// <summary>
        /// path=/,root=kuaipan时不返回。folder为文件夹，file为文件。
        /// </summary>
        public TypeEnum Type { get; private set; }

        /// <summary>
        /// path=/,root=kuaipan时不返回。文件大小。
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// path=/,root=kuaipan时不返回。YYYY-MM-DD hh:mm:ss。
        /// </summary>
        public DateTime Createtime { get; private set; }

        /// <summary>
        /// path=/,root=kuaipan时不返回。YYYY-MM-DD hh:mm:ss。
        /// </summary>
        public DateTime ModifyTime { get; private set; }

        /// <summary>
        /// path=/，root=kuaipan时不返回。文件名。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// path=/,root=kuaipan时不返回。
        /// </summary>
        public string Rev { get; private set; }

        /// <summary>
        /// path=/，root=kuaipan时不返回。是否被删除的文件。
        /// </summary>
        public string IsDeleted { get; private set; }

        //public PHPArray Files { get; private set; }

        public List<FileData> Files { get; private set; }

        public override string ToString()
        {
            return string.Format("MetaData [Path={0}, Root={1}, Hash={2}, FileId={3}, Type={4}, Size={5}, Createtime={6}, ModifyTime={7}, Name={8}, Rev={9}, IsDeleted={10}, FilesCount={11}]",
                this.Path, this.Root, this.Hash, this.FileId, this.Type, this.Size, this.Createtime, this.ModifyTime, this.Name, this.Rev, this.IsDeleted, this.Files.Count);
        }
    }
}
