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
using PHPLibrary;

namespace KuaiPanSDK.Model
{
    public class Create
    {
        public Create(PHPArray data)
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
            this.FileId = this.Data["file_id"];
            this.Msg = this.Data["msg"];
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
        /// 
        /// </summary>
        public string FileId { get; private set; }

        /// <summary>
        /// msg
        /// </summary>
        public string Msg { get; private set; }

        public override string ToString()
        {
            return string.Format("Create [Root={0}, Path={1}, FileId={2}, Msg={3}]",
                this.Root, this.Path, this.FileId, this.Msg);
        }
    }
}
