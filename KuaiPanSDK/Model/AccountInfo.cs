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
    /// <summary>
    /// 查看用户信息
    /// </summary>
    public class AccountInfo
    {
        public AccountInfo(PHPArray data)
        {
            this.Data = data;
            this.SetProperty();
        }

        /// <summary>
        /// 设置属性
        /// </summary>
        private void SetProperty()
        {
            this.UserId = this.Data["user_id"];
            this.UserName = this.Data["user_name"];
            this.MaxFileSize = this.Data["max_file_size"];
            this.QuotaTotal = this.Data["quota_total"];
            this.QuotaUsed = this.Data["quota_used"];
            this.QuotaRecycled = this.Data["quota_recycled"];
        }

        /// <summary>
        /// 数据
        /// </summary>
        public PHPArray Data { get; private set; }

        /// <summary>
        /// 标识用户的唯一id
        /// </summary>
        public int UserId { get; private set; }

        /// <summary>
        /// 用户名字
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// 允许上传最大文件
        /// </summary>
        public long MaxFileSize { get; private set; }

        /// <summary>
        /// 用户空间总量,Byte
        /// </summary>
        public long QuotaTotal { get; private set; }

        /// <summary>
        /// 用户空间已使用量,Byte
        /// </summary>
        public long QuotaUsed { get; private set; }

        /// <summary>
        /// 用户空间的回收站空间使用量，Byte
        /// </summary>
        public long QuotaRecycled { get; private set; }

        public override string ToString()
        {
            return string.Format("AccountInfo [UserId={0}, UserName={1}, MaxFileSize={2}({6}), QuotaTotal={3}({7}), QuotaUsed={4}({8}), QuotaRecycled={5}({9})]",
                this.UserId, this.UserName, this.MaxFileSize, this.QuotaTotal, this.QuotaUsed, this.QuotaRecycled, Common.CountSize(this.MaxFileSize), Common.CountSize(this.QuotaTotal), Common.CountSize(this.QuotaUsed), Common.CountSize(this.QuotaRecycled));
        }
    }
}
