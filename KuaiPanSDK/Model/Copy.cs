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
    public class Copy
    {
        public Copy(PHPArray data)
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
        }

        /// <summary>
        /// 数据
        /// </summary>
        public PHPArray Data { get; private set; }

        /// <summary>
        /// FileId
        /// </summary>
        public string FileId { get; private set; }

        public override string ToString()
        {
            return string.Format("Copy [FileId={0}]",
                this.FileId);
        }
    }
}
