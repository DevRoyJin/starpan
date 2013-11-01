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
using PHPLibrary;

namespace KuaiPanSDK.Model
{
    public class FileData
    {
        public FileData(PHPArray data)
        {
            this.Data = data;
            this.SetProperty();
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        private void SetProperty()
        {
            this.FileId = this.Data["file_id"];
            this.Type = (string)this.Data["type"] == "file" ? TypeEnum.File : (string)this.Data["type"] == "folder" ? TypeEnum.Folder : TypeEnum.Root;
            this.Size = this.Data["size"];
            this.Createtime = this.Data["create_time"];
            this.ModifyTime = this.Data["modify_time"];
            this.Name = this.Data["name"];
            this.Rev = this.Data["rev"];
            this.IsDeleted = this.Data["is_deleted"];
        }

        /// <summary>
        /// 数据
        /// </summary>
        public PHPArray Data { get; private set; }

        /// <summary>
        /// 文件唯一标识id
        /// </summary>
        public string FileId { get; private set; }

        /// <summary>
        /// folder为文件夹，file为文件。
        /// </summary>
        public TypeEnum Type { get; private set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// YYYY-MM-DD hh:mm:ss
        /// </summary>
        public DateTime Createtime { get; private set; }

        /// <summary>
        /// YYYY-MM-DD hh:mm:ss
        /// </summary>
        public DateTime ModifyTime { get; private set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Rev { get; private set; }

        /// <summary>
        /// 是否被删除的文件
        /// </summary>
        public string IsDeleted { get; private set; }

        public override string ToString()
        {
            return string.Format("FileInfo [FileId={0}, Type={1}, Size={2}, Createtime={3}, ModifyTime={4}, Name={5}, Rev={6}, IsDeleted={7}]",
                this.FileId, this.Type, this.Size, this.Createtime, this.ModifyTime, this.Name, this.Rev, this.IsDeleted);
        }
    }
}
