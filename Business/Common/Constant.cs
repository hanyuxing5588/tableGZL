using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Common
{
    public class Constant
    {
        #region 旧系统中存在的状态
        /// <summary>
        /// 未审批（旧系统中）
        /// </summary>
        public const string NotApproveState = ",0";
        /// <summary>
        /// 已经审批
        /// </summary>
        public const string ApprovedState = "999,-1";
        
        #endregion
        #region 新信息系统中的状态变量
        //ApproveState 字段审批状态 0未提交 1未审批 2审批中 3已经审批  
        /// <summary>
        /// 未提交
        /// </summary>
        public const string NotSubmitState = "0";
        /// <summary>
        /// 未审批
        /// </summary>
        public const string NewNotApproveState = "1";
        /// <summary>
        /// 审批中
        /// </summary>
        public const string NewApprovingState = "2";
        /// <summary>
        /// 审批完成
        /// </summary>
        public const string NewApprovedState = "3";

        #endregion

        #region 业务类型
          /// <summary>
            /// 预算管理
            /// </summary>
           public const string YWOne="01";
            /// <summary>
            /// 报销管理
            /// </summary>
            public const string YWTwo="02";
            /// <summary>
            /// 收入管理
            /// </summary>
            public const string YWThree="03";
            /// <summary>
            /// 专用基金
            /// </summary>
            public const string YWFour="04";
            /// <summary>
            /// 往来管理
            /// </summary>
            public const string YWFive="05";
            /// <summary>
            /// 单位往来
            /// </summary>
           public const string YWFiveO="0501";
            /// <summary>
            /// 个人往来
            /// </summary>
           public const string YWFiveT="0502";
            /// <summary>
            /// 资产管理
            /// </summary>
            public const string YWSix="06";
            /// <summary>
            /// 薪资管理
            /// </summary>
            public const string YWSeven="07";
            /// <summary>
            /// 出纳管理
            /// </summary>
            public const string YWEight="08";
            /// <summary>
            /// 收付款管理
            /// </summary>
            public const string YWEightO="0801";
            /// <summary>
            /// 提存现管理
            /// </summary>
            public const string YWEightT="0802";
            /// <summary>
            /// 核销管理
            /// </summary>
            public const string YWEightTh="0803";
            /// <summary>
            /// 财政支付码
            /// </summary>
            public const string YWEightF = "0804";
            /// <summary>
            /// 支票管理
            /// </summary>
            public const string YWEightFive = "0805";
            /// <summary>
            /// 会计核算
            /// </summary>
            public const string YWNine = "09";
            /// <summary>
            /// 薪资管理
            /// </summary>
            public const string YWNineO = "0901";
            /// <summary>
            /// 核算管理
            /// </summary>
            public const string YWNineT = "0902";        
            /// <summary>
            /// 核销管理（未使用）
            /// </summary>
            public const string YWTen = "10";
            /// <summary>
            /// OA办公
            /// </summary>
            public const string YWEleven = "11";
            /// <summary>
            /// 收入信息流转
            /// </summary>
            public const string YWElevenO = "1101";
            /// <summary>
            /// 货品管理
            /// </summary>
            public const string YWTwelve = "12";
            /// <summary>
            /// 货品代购
            /// </summary>
            public const string YWTwelveO = "1201";
            /// <summary>
            ///货品领用
            /// </summary>
            public const string YWTwelveT = "1202";
        #endregion

        #region 预算类型
       
        /// <summary>
        /// 基本支出
        /// </summary>
        public const string BGTypeOne="01";
        /// <summary>
        /// 项目支出
        /// </summary>
        public const string BGTypeTwo = "02";
        #endregion

        #region 基础----人员类别
        ///zzp  04-24
        /// <summary>
        /// 01--在编
        /// </summary>
        public const string PersonTypeOne = "01";
        /// <summary>
        /// 02--离职
        /// </summary>
        public const string PersonTypeTwo = "02";
        /// <summary>
        /// 03--离退休
        /// </summary>
        public const string PersonTypeThree = "03";
        /// <summary>
        /// 04--外聘
        /// </summary>
        public const string PersonTypeFour = "04";
        /// <summary>
        /// 05--其他
        /// </summary>
        public const string PersonTypeFive = "05";


        #endregion

        #region 基础----证件类型

        ///zzp  2014-04-24  11:49
        /// <summary>
        /// 01--身份证
        /// </summary>
        public const string CredentialTypeOne = "01";
        /// <summary>
        /// 02--军官证
        /// </summary>
        public const string CredentialTypeTwo = "02";
        /// <summary>
        /// 03--护照
        /// </summary>
        public const string CredentialTypeThree = "03";
        /// <summary>
        /// 04--港澳通行证
        /// </summary>
        public const string CredentialTypeFour = "04";
        /// <summary>
        /// 05--其他
        /// </summary>
        public const string CredentialTypeFive = "05";

        #endregion

        /// <summary>
        /// 未审批状态
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<string> ListNotApproveState()
        {
            List<string> list = new List<string>();
            list.Add(Constant.NotSubmitState);//未提交
            list.Add(Constant.NewNotApproveState);//未审批
            list.Add("");//空值
            list.Add(null);//空值
            return list;
        }
        /// <summary>
        /// 已经提交的状态
        /// </summary>
        /// <returns></returns>
        public static List<string> ListSubmitState()
        {
            List<string> approveStateList = new List<string>();
            approveStateList.Add(Constant.NewNotApproveState);// 1 未审批
            approveStateList.Add(Constant.NewApprovingState);//2 审批中
            approveStateList.Add(Constant.NewApprovedState);// 3已审批
            return approveStateList;
        }
    }
    /// <summary>
    /// 扩展
    /// </summary>
    public static class ConstantExtension
    {
        /// <summary>
        /// 单据状态
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static List<string> ListState(this string obj)
        {
            List<string> list = new List<string>();
            if (obj.IndexOf(',') >= 0)
            {
                list.AddRange(obj.Split(',').ToList());
            }
            else
            {
                list.Add(obj);
            }
            return list;
        }
        /// <summary>
        /// 单价int?状态
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<int?> ListIntState(this string obj)
        {
            List<int?> list = new List<int?>();
            int g;
            if (obj.IndexOf(',') >= 0)
            {
                var arr = obj.Split(',');               
                for (int i = 0; i < arr.Length; i++)
                {
                    if (string.IsNullOrEmpty(arr[i]))
                    {
                        list.Add(null);
                    }
                    else
                    {
                        if (int.TryParse(arr[i],out g))
                        {
                            list.Add(g);
                        }
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(obj))
                {
                    list.Add(null);
                }
                else
                {
                    if (int.TryParse(obj, out g))
                    {
                        list.Add(g);
                    }
                }
            }
            return list;
        }
       
        public static bool ExistState(this string obj, string state,int position)
        {
            var stateValue = state.Substring(position,1);
            if (obj == stateValue)
            {
                return true;
            }
            return false;
        }
    }
}
