using Commons;
using Commons.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Emailer.Smtp
{
    public class Emailer : Interface_Emailer
    {
        public EmailResponse Send_Email(EmailParams Params)
        {
            EmailResponse Response = new EmailResponse();

            try
            {
                var SmtpHost = Params.Smtp_Host; 

                SmtpClient Smtp = new SmtpClient();
                Smtp.Host = SmtpHost;
                Smtp.UseDefaultCredentials = true;
                Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                Smtp.Timeout = 20000;

                MailMessage Msg = new MailMessage();
                Msg.Subject = Params.Subject;
                Msg.From =
                    new MailAddress(
                        CommonMethods.Convert_String(Params.Sender_Email)
                        , CommonMethods.Convert_String(Params.Sender_Display));

                if (Params.Recepients_To != null)
                {
                    //Msg.To.Add(String.Join(",", Params.Recepients_To.Where(O => !String.IsNullOrEmpty(O)))); 

                    Params.Recepients_To
                        .Where(O => !String.IsNullOrEmpty(O))
                        .ToList()
                        .ForEach(O_Recepient => {
                            O_Recepient
                                .Split(',')
                                .Where(O => !String.IsNullOrEmpty(O))
                                .ToList()
                                .ForEach(O_Recepient_Splitted => { Msg.To.Add(O_Recepient_Splitted); });
                        });

                }

                if (Params.Recepients_Cc != null)
                { 
                    //Msg.CC.Add(String.Join(",", Params.Recepients_Cc.Where(O => !String.IsNullOrEmpty(O))));

                    Params.Recepients_Cc
                       .Where(O => !String.IsNullOrEmpty(O))
                       .ToList()
                       .ForEach(O_Recepient => {
                           O_Recepient
                               .Split(',')
                               .Where(O => !String.IsNullOrEmpty(O))
                               .ToList()
                               .ForEach(O_Recepient_Splitted => { Msg.CC.Add(O_Recepient_Splitted); });
                       });

                }

                if (Params.Recepients_Bcc != null)
                {
                    //Msg.Bcc.Add(String.Join(",", Params.Recepients_Bcc.Where(O => !String.IsNullOrEmpty(O)))); 

                    Params.Recepients_Bcc
                       .Where(O => !String.IsNullOrEmpty(O))
                       .ToList()
                       .ForEach(O_Recepient => {
                           O_Recepient
                               .Split(',')
                               .Where(O => !String.IsNullOrEmpty(O))
                               .ToList()
                               .ForEach(O_Recepient_Splitted => { Msg.Bcc.Add(O_Recepient_Splitted); });
                       });

                }

                Msg.Body = Params.Body;
                Msg.IsBodyHtml = Params.IsBodyHtml;

                if (Params.Attachment_FilePaths != null)
                {
                    Params.Attachment_FilePaths.ForEach(O => {
                        Msg.Attachments.Add(new Attachment(O));
                    });
                }

                Msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                Smtp.Send(Msg);

                Response.Result = true;
            }
            catch (Exception Ex)
            {
                String ExceptionMsg =
$@"
Emailer Exception Caught.
Parameters: 
{Serializer.SerializeToString(Serializer.SerializerType.Json, Params) }";
                Exception Ex_Extended = new Exception(ExceptionMsg, Ex);

                Response.SimpleException = new SimpleException(Ex_Extended);
                Response.Result = false;                
            }

            return Response;
        }
    }
}
