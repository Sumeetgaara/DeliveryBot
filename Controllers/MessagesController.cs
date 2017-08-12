using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using AdaptiveCards;

namespace Delivery_Bot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity != null && activity.GetActivityType() == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new BotDialog());
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
    //main dialog 
    [Serializable]
    public class BotDialog : IDialog<object>
    {

        public string orderName { get; set; }
        public string status { get; set; }
        public string date { get; set; }

        public string toId { get; set; }
        public string toName { get; set; }

        public string fromId { get; set; }
        public string fromName { get; set; }

        public string serviceURL { get; set; }
        public string channelID { get; set; }

        public string conversationID { get; set; }

        //waits for user to reply
        public async Task StartAsync(IDialogContext context)
        {

            context.Wait(usertBot);
        }
        public virtual async Task usertBot(IDialogContext context, IAwaitable<IMessageActivity> argument)//decides type of user
        {
            var value = await argument;
            string choice = Convert.ToString(value.Text);
            if (choice == "Admin")
            {
                await context.PostAsync("Bro code?");
                context.Wait(pBot);

            }
            else
            {
                await context.PostAsync("Hi Awesome ! I am delivery bot!Just tell me your order number");
                context.Wait(oBot);
            }
        }
        public virtual async Task pBot(IDialogContext context, IAwaitable<IMessageActivity> argument)//admin confirmation
        {
            var value = await argument;
            string choice = Convert.ToString(value.Text);
            Models.DBotDBEntities db = new Models.DBotDBEntities();
            var broCode = (from adminTable in db.adminTable1
                           where adminTable.username=="Admin"
                           select adminTable.pwd);
            string broCode1 = broCode.Single();
            if (choice == broCode1)
            {
                await context.PostAsync("Hey sumi!!!Do I need to broadcast notification");
                context.Wait(aBot);
            }
            else
            {
                await context.PostAsync("Sorry!you are not an admin");
                context.Wait(usertBot);
            }

        }
        public async Task aBot(IDialogContext context, IAwaitable<IMessageActivity> argument)//broadcasts the message
        {
            var result = await argument;
            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Attachments = new List<Attachment>();
            AdaptiveCard card = new AdaptiveCard();


            Models.DBotDBEntities db = new Models.DBotDBEntities();
            var sendLists = (from DbotTable in db.DbotTables
                             where DbotTable.conversationID != null
                             select DbotTable)
                            .ToList();
            foreach (var sendList in sendLists)
            {
                toId = sendList.toID;
                toName = sendList.toName;
                fromId = sendList.fromID;
                fromName = sendList.fromName;
                serviceURL = sendList.serviceURL;
                conversationID = sendList.conversationID;
                channelID = sendList.channelID;
                status = sendList.Status;
                date = sendList.Date;

                var userAccount = new ChannelAccount(toId, toName);
                var botAccount = new ChannelAccount(fromId, fromName);
                var connector = new ConnectorClient(new Uri(serviceURL));

                IMessageActivity message = Activity.CreateMessageActivity();
                if (!string.IsNullOrEmpty(conversationID) && !string.IsNullOrEmpty(channelID))
                {
                    
                    replyToConversation.ChannelId = channelID;
                }
                else
                {
                    conversationID = (await connector.Conversations.CreateDirectConversationAsync(botAccount, userAccount)).Id;
                }

                // Set the address-related properties in the message and send the message.
                //message.From = botAccount;
                //message.Recipient = userAccount;
                //message.Conversation = new ConversationAccount(id: conversationID);
                //message.Text = "Delivery status: " + status + " .Delivery date: " + date;
                //message.Locale = "en-Us";
                replyToConversation.From = botAccount;
                replyToConversation.Recipient = userAccount;
                replyToConversation.Conversation = new ConversationAccount(id: conversationID);
                replyToConversation.Locale = "en-Us";

                card.Body.Add(new TextBlock() 
                {
                    Text = "Daily Update",
                    Size = TextSize.Large,
                    Weight = TextWeight.Bolder
                });
                card.Body.Add(new TextBlock()
                {
                    Text = "Delivery status: " + status
                });
                card.Body.Add(new TextBlock()
                {
                    Text = "Delivery Date: " + date
                });

                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
                card.Speak = "Update coming";
                replyToConversation.Attachments.Add(attachment);
                

                await connector.Conversations.SendToConversationAsync(replyToConversation);


            }

        }


        //public virtual async Task wBot(IDialogContext context, IAwaitable<IMessageActivity> argument)
        //{
        //    await context.PostAsync("Hi Awesome ! I am delivery bot!Just tell me your order number");
        //    context.Wait(oBot);
        //}

        public virtual async Task oBot(IDialogContext context, IAwaitable<IMessageActivity> argument)//display the status
        {
            var value = await argument;
            var connector = new ConnectorClient(new Uri(value.ServiceUrl));
            orderName = Convert.ToString(value.Text);
            Models.DBotDBEntities db = new Models.DBotDBEntities();
            var sStatus = (from DbotTable in db.DbotTables
                           where DbotTable.OrderNumber == orderName
                           select DbotTable.Status);
            status = Convert.ToString(sStatus.FirstOrDefault());
            var dDate = (from DbotTable in db.DbotTables
                         where DbotTable.OrderNumber == orderName
                         select DbotTable.Date);
            date = Convert.ToString(dDate.FirstOrDefault());
            if (status == null || date == null)
            {
                await context.PostAsync("No such record");
                context.Wait(usertBot);
            }
            else
            {
                Activity replyToConversation = (Activity)context.MakeMessage();
                replyToConversation.Attachments = new List<Attachment>();
                AdaptiveCard card = new AdaptiveCard();
                card.Body.Add(new TextBlock() 
                {
                    Text = "Result",
                    Size = TextSize.Large,
                    Weight = TextWeight.Bolder
                });
                card.Body.Add(new TextBlock()
                {
                    Text = "Delivery status: " + status
                });
                card.Body.Add(new TextBlock()
                {
                    Text = "Delivery Date: " + date
                });

                Attachment attachment = new Attachment()
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
                card.Speak = "Update coming";
                replyToConversation.Attachments.Add(attachment);
                await connector.Conversations.SendToConversationAsync(replyToConversation);
                context.Wait(msBot);
            }

        }
        public virtual async Task msBot(IDialogContext context, IAwaitable<IMessageActivity> argument)//decide whether to get added or not
        {
            var lol = await argument;
            var connector = new ConnectorClient(new Uri(lol.ServiceUrl));
            Activity replyToConversation = (Activity)context.MakeMessage();
            replyToConversation.Attachments = new List<Attachment>();

            List<CardAction> cardButtons = new List<CardAction>();

            CardAction plButton = new CardAction()
            {
                Value = 1.ToString(),
                Title = "Yes",
                Type = "imBack"

            };

            cardButtons.Add(plButton);
            CardAction plButton1 = new CardAction()
            {
                Value = 2.ToString(),
                Title = "No",
                Type="imBack"

            };
            cardButtons.Add(plButton1);
            HeroCard plCard = new HeroCard()
            {
                Title = "You want to be part of our family?",
                Buttons = cardButtons
            };
            
            Attachment plAttachment = plCard.ToAttachment();
            replyToConversation.Attachments.Add(plAttachment);
            await connector.Conversations.SendToConversationAsync(replyToConversation);
            context.Wait(sBot);
        }



        public virtual async Task sBot(IDialogContext context, IAwaitable<IMessageActivity> argument)//subsrcibing action.
        {
            var value = await argument;
            toId = value.From.Id;
            toName = value.From.Name;
            fromId = value.Recipient.Id;
            fromName = value.Recipient.Name;
            serviceURL = value.ServiceUrl;
            channelID = value.ChannelId;
            conversationID = value.Conversation.Id;

            string choice = Convert.ToString(value.Text).ToUpper();
            if (choice == "1")
            {
                Models.DBotDBEntities db = new Models.DBotDBEntities();
                Models.DbotTable tb = db.DbotTables.Single(x => x.OrderNumber == orderName);
                tb.fromID = fromId;
                tb.fromName = fromName;
                tb.toID = toId;
                tb.toName = toName;
                tb.serviceURL = serviceURL;
                tb.channelID = channelID;
                tb.conversationID = conversationID;
                db.SaveChanges();
                await context.PostAsync("Yaya!Added");

            }
            else
            {
                await context.PostAsync("No problem!");
            }
            context.Wait(usertBot);
        }

    }
}
    
