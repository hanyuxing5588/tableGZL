using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using BusinessModel;
using Infrastructure;
using System.Collections;

namespace Business
{

    public class EmailDrive
    {
        

        private SmtpClient client = null;

        public EmailDrive(string Host,string HostLogonName,string HostPassword)
        {
            client = new SmtpClient();
            //设置用于 SMTP 事务的主机的名称，填IP地址也可以了
            client.Host = Host;//"smtp.hotmail.com";
            //设置用于 SMTP 事务的端口，默认的是 25
            //client.Port = 25;
            client.UseDefaultCredentials = false;
            //这里才是真正的邮箱登陆名和密码，比如我的邮箱地址是 hbgx@hotmail， 我的用户名为 hbgx ，我的密码是 xgbh
            client.Credentials = new System.Net.NetworkCredential(HostLogonName, HostPassword);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
        }

        public void SendMail(string SubjectName,string FromAddress,string FromDisplayName,
            string ToAddress,string ToDisplayName,string Content,bool EnableSsl=false,int Port=25)
        {

            if (string.IsNullOrEmpty(FromDisplayName.Trim())) FromDisplayName = FromAddress;
            if (string.IsNullOrEmpty(ToDisplayName.Trim())) ToDisplayName = ToAddress;

            MailAddress MailFrom = new MailAddress(FromAddress, FromDisplayName, System.Text.Encoding.UTF8);
            MailAddress MailTo = new MailAddress(ToAddress, ToDisplayName, System.Text.Encoding.UTF8);
            MailMessage mail = new MailMessage();
            mail.From = MailFrom;
            mail.To.Add(MailTo);
            mail.Subject = SubjectName;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = Content;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;
            this.client.Port = Port;
            this.client.EnableSsl = EnableSsl;
            this.client.Send(mail);
        }

        
    }

    public class SADrive
    {
        private BusinessModel.BusinessEdmxEntities _bcontext = null;
        public BusinessModel.BusinessEdmxEntities BusinessContext
        {
            get
            {
                if (_bcontext == null) _bcontext = new BusinessEdmxEntities();
                return _bcontext;
            }
        }

        private Infrastructure.BaseConfigEdmxEntities _icontext = null;
        public Infrastructure.BaseConfigEdmxEntities InfrastructureContext
        {
            get
            {
                if (_icontext == null) _icontext = new BaseConfigEdmxEntities();
                return _icontext;
            }
        }
        public SAPerson RetrievePersonSA(Guid PersonId, int ActionYear, int ActionMonth, string ActionState = "1")
        {
            string Sql = "select ItemKey,ItemName,ItemValue,ItemDatetime,ItemString,PlanKey,PlanName,a.PersonKey,a.PersonName,c.Email,ItemFormula,ItemDefaultFormula from ("
                      + "select * from sa_planactiondetailview where guid_person='{0}'"
                      + " and actionyear={1} and actionmouth={2}"
                      + " and GUID_PlanAction in (select guid from sa_planaction where ActionState={3})"
                      + ") a left join sa_planitem b on a.guid_item=b.guid_item and a.guid_plan=b.guid_plan"
                      + " left join SS_Person c on a.PersonKey=c.PersonKey"
                      + " order by plankey, itemordernum";
            Sql = string.Format(Sql, PersonId, ActionYear, ActionMonth, ActionState);

            var details = this.BusinessContext.ExecuteStoreQuery<SACustomItem>(Sql).ToList();

            SAPerson result = new SAPerson();
            result.Year = ActionYear.ToString();
            result.Month = ActionMonth.ToString();
            
            if (details == null || details.Count == 0) return result;

            List<string> plankeys = new List<string>();
            foreach (SACustomItem dr in details)
            {
                if (!plankeys.Contains(dr.PlanKey))
                {
                    result.PersonName = dr.PersonName;
                    result.Email = dr.Email;
                    SAPlanAction actionitem = new SAPlanAction();
                    actionitem.PlanKey = dr.PlanKey;
                    actionitem.PlanName = dr.PlanName;
                    actionitem.Items = details.FindAll(e => e.PlanKey == dr.PlanKey);
                    result.ActionItems.Add(actionitem);
                    plankeys.Add(dr.PlanKey);
                }
            }

            return result;
        }

        public List<SAPerson> RetrieveSA(int ActionYear, int ActionMonth, string ActionState = "1")
        {
            string Sql = "select ItemKey,ItemName,ItemValue,ItemDatetime,ItemString,PlanKey,PlanName,a.PersonKey,a.PersonName,c.Email,ItemFormula,ItemDefaultFormula from ("
                      + "select * from sa_planactiondetailview where"
                      + " actionyear={0} and actionmouth={1}"
                      + " and GUID_PlanAction in (select guid from sa_planaction where ActionState={2})"
                      + ") a left join sa_planitem b on a.guid_item=b.guid_item and a.guid_plan=b.guid_plan"
                      + " left join SS_Person c on a.PersonKey=c.PersonKey"
                      + " order by plankey, itemordernum";
            Sql = string.Format(Sql, ActionYear, ActionMonth, ActionState);

            var details = this.BusinessContext.ExecuteStoreQuery<SACustomItem>(Sql).ToList();

            List<SAPerson> results = new List<SAPerson>();



            if (details == null || details.Count == 0) return results;

            List<string> personkeys = new List<string>();
            foreach (SACustomItem dr in details)
            {
                if (!personkeys.Contains(dr.PersonKey))
                {
                    SAPerson result = new SAPerson();
                    result.Year = ActionYear.ToString();
                    result.Month = ActionMonth.ToString();
                    result.PersonName = dr.PersonName;
                    result.Email = dr.Email;
                    var personitems = details.FindAll(e => e.PersonKey == dr.PersonKey);
                    if (personitems != null && personitems.Count > 0)
                    {
                        List<string> plankeys = new List<string>();
                        foreach (SACustomItem pdr in personitems)
                        {
                            if (!plankeys.Contains(pdr.PlanKey))
                            {
                                SAPlanAction actionitem = new SAPlanAction();
                                actionitem.PlanKey = pdr.PlanKey;
                                actionitem.PlanName = pdr.PlanName;
                                actionitem.Items = personitems.FindAll(e => e.PlanKey == dr.PlanKey);
                                result.ActionItems.Add(actionitem);

                                plankeys.Add(dr.PlanKey);
                            }
                        }
                    }

                    results.Add(result);
                    personkeys.Add(dr.PersonKey);
                }
            }

            return results;
        }
    }

    public class SACustomItem
    {
        public string ItemKey { get; set; }
        public string ItemName { get; set; }
        public double? ItemValue { get; set; }
        public DateTime? ItemDatetime { get; set; }
        public string ItemString { get; set; }
        public string PlanKey { get; set; }
        public string PlanName { get; set; }
        public string PersonKey { get; set; }
        public string PersonName { get; set; }
        public string Email { get; set; }
        public string ItemFormula { get; set; }
        public string ItemDefaultFormula { get; set; }
    }

    public class SAPlanAction
    {
        public string PlanName { get; set; }
        public string PlanKey { get; set; }
        public List<SACustomItem> Items { get; set; }

        public SAPlanAction() { this.Items = new List<SACustomItem>(); }
    }

    public class SAPerson
    {
        public string PersonName { get; set; }
        public string Year { get; set; }
        public string Month { get; set; }
        public string Email { get; set; }
        public List<SAPlanAction> ActionItems { get; set; }

        public SAPerson() { this.ActionItems = new List<SAPlanAction>(); }

        public string ConvertToHtml()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("<html>");
            
            foreach (SAPlanAction item in ActionItems)
            {
                result.AppendLine(ConvertEx(item));
                result.AppendLine("<br><br>");
            }
            result.AppendLine(AddtionInformation());
            result.AppendLine("<br><br>");
            result.AppendLine("</html>");
            return result.ToString();
        }

        private string Convert(SAPlanAction action)
        {
            if (action.Items.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<table style='margin:0;padding:0;' cellpadding='0' cellspacing='0' border='1'>");
            string rowtitle = string.Empty, rowname = string.Empty, rowitem = string.Empty, rowvalue = string.Empty;
            rowtitle = string.Format("<tr><td align='left' style='border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;' colspan='{0}'>{1}</td></tr>",
                action.Items.Count + 1, Year + "年" + Month + "月" + action.PlanName);
            rowname = string.Format("<tr><td align='left' style='border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;' colspan='{0}'>姓名:{1}</td></tr>",
                action.Items.Count + 1, PersonName);


            rowitem = "<tr><td align='center' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;'>项目</td>";
            rowvalue = "<tr><td align='center' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;'>金额</td>";

            foreach (var item in action.Items)
            {
                string value = item.ItemValue != null ? string.Format("{0:N}", item.ItemValue) :
                    item.ItemDatetime != null ? item.ItemDatetime.ToString() :
                    !string.IsNullOrEmpty(item.ItemString) ? item.ItemString : string.Empty;
                bool HasGongShi = !string.IsNullOrEmpty(item.ItemFormula) || !string.IsNullOrEmpty(item.ItemDefaultFormula);

                string bkcolor = HasGongShi ? "background-color:Aqua;" : "";

                if (value == "0.00") value = string.Empty;
                rowitem = rowitem + string.Format("<td align='center' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-right:1px;{1}'>{0}</td>",
                    item.ItemName, bkcolor);
                rowvalue = rowvalue + string.Format("<td align='center' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-right:1px;{1}'>{0}</td>",
                    value, bkcolor);
            }
            rowitem += "</tr>"; rowvalue += "</tr>";
            sb.AppendLine(rowtitle);
            sb.AppendLine(rowname);
            sb.AppendLine(rowitem);
            sb.AppendLine(rowvalue);
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private string ConvertEx(SAPlanAction action)
        {
            if (action.Items.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<table style='margin:0;padding:0;' cellpadding='0' cellspacing='0' border='1'>");
            string rowtitle = string.Empty, rowname = string.Empty, rowitem = string.Empty, rowvalue = string.Empty;
            rowtitle = string.Format("<tr><td align='left' style='border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;' colspan='2'>{0}</td></tr>",
                Year + "年" + Month + "月" + action.PlanName);
            rowname = string.Format("<tr><td align='left' style='border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;' colspan='2'>姓名:{0}</td></tr>",
                PersonName);
            sb.AppendLine(rowtitle);
            sb.AppendLine(rowname);

            var rowitemtitle = "<tr><td align='center' valign='middle' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-left:1px;border-right:1px;background-color:#fba949;'>工资项</td>" +
                               "<td align='center' valign='middle' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-right:1px;background-color:#fba949;'>金额(元)</td></tr>";

            sb.AppendLine(rowitemtitle);
            foreach (var item in action.Items)
            {
                string value = item.ItemValue != null ? string.Format("{0:N}", item.ItemValue) :
                    item.ItemDatetime != null ? item.ItemDatetime.ToString() :
                    !string.IsNullOrEmpty(item.ItemString) ? item.ItemString : string.Empty;
                bool HasGongShi = !string.IsNullOrEmpty(item.ItemFormula) || !string.IsNullOrEmpty(item.ItemDefaultFormula);

                string bkcolor = HasGongShi ? "background-color:Aqua;" : "";

                if (value == "0.00") value = string.Empty;

                rowitem = string.Format("<tr><td align='left' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-left:1px;border-right:1px;{2}'>{0}</td>"
                    + "<td align='right' nowrap='nowrap' style='white-space:nowrap;border-top:1px;border-right:1px;{2}'>{1}</td></tr>", item.ItemName
                    , value, bkcolor);
                sb.AppendLine(rowitem);
            }
            sb.AppendLine("</table>");
            return sb.ToString();
        }

        private string AddtionInformation()
        {
            StringBuilder sb =new StringBuilder();
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;border-width: 0 0 1px 0;position: relative;margin-left: 0px;margin-right: 0px;'>说明：</dl>");
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;border-width: 0 0 1px 0;position: relative;margin-left: 0px;margin-right: 0px;'>一、计税额=应发合计+其他计税额-免税额-扣款合计。</dl>");
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;border-width: 0 0 1px 0;position: relative;margin-left: 0px;margin-right: 0px;'>其中：1、免税额=政府特贴+人才特贴+电话补助+独补+托补+购房补贴+房租补+补发+其他+物业服务补贴。</dl>");
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;border-width: 0 0 1px 0;position: relative;margin-left: 0px;margin-right: 0px;'><p>2、扣款合计=扣款+扣房金+扣失业保险+扣医疗保险+扣养老保险。</p></dl>");
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;border-width: 0 0 1px 0;position: relative;margin-left: 0px;margin-right: 0px;'>二、个人所得税=（计税额-3500）*税率-速算扣除数。</dl>");
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;border-width: 0 0 1px 0;position: relative;margin-left: 0px;margin-right: 0px;'></dl>");
            sb.AppendLine("<dl style='border-bottom: 1px solid #e1e1e1;position: relative;margin-left: 0px;margin-right: 0px;text-align: center;border-left-width: 0;border-right-width: 0;border-top-width: 0;'>工资、薪金所得适用个人所得税累进税率表</dl>");
            
            sb.AppendLine("<table id='ssTable' style=' border:1px solid black;'>");
            sb.AppendLine("<tr><td rowspan='2' class='style1'>级数</td><td colspan='2' class='style1'>全月应纳税所得额</td>");
            sb.AppendLine("<td rowspan='2' class='style1'>税率(%)</td>");
            sb.AppendLine("<td rowspan='2' class='style1'>速算扣除数</td></tr>");
            sb.AppendLine("<tr><td class='style1'>含税级距</td><td class='style1'>不含税级距</td></tr></tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>1</td>");
            sb.AppendLine("<td>不超过1500元的</td>");
            sb.AppendLine("<td>不超过1455元的</td>");
            sb.AppendLine("<td>3</td>");
            sb.AppendLine("<td>0</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>2</td>");
            sb.AppendLine("<td>超过1500元至4500元的部分</td>");
            sb.AppendLine("<td>超过1455元至4155元的部分</td>");
            sb.AppendLine("<td>10</td>");
            sb.AppendLine("<td>105</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>3</td>");
            sb.AppendLine("<td>超过4500元至9000元的部分</td>");
            sb.AppendLine("<td>超过4155元至7755元的部分</td>");
            sb.AppendLine("<td>20</td>");
            sb.AppendLine("<td>555</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>4</td>");
            sb.AppendLine("<td>超过9000元至35000元的部分</td>");
            sb.AppendLine("<td>超过7755元至27255元的部分</td>");
            sb.AppendLine("<td>25</td>");
            sb.AppendLine("<td>1005</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>5</td>");
            sb.AppendLine("<td>超过35000元至55000元的部分</td>");
            sb.AppendLine("<td>超过27255元至41255元的部分</td>");
            sb.AppendLine("<td>30</td>");
            sb.AppendLine("<td>2755</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>6</td>");
            sb.AppendLine("<td>超过55000元至80000元的部分</td>");
            sb.AppendLine("<td>超过41255元至57505元的部分</td>");
            sb.AppendLine("<td>35</td>");
            sb.AppendLine("<td>5505</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine("<td>7</td>");
            sb.AppendLine("<td>超过80000元的部分</td>");
            sb.AppendLine("<td>超过57505元的部分</td>");
            sb.AppendLine("<td>45</td>");
            sb.AppendLine("<td>13505</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            return sb.ToString();
        }
    }

}
