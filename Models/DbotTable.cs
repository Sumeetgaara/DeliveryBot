//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Delivery_Bot.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class DbotTable
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }
        public string toID { get; set; }
        public string toName { get; set; }
        public string fromID { get; set; }
        public string fromName { get; set; }
        public string serviceURL { get; set; }
        public string channelID { get; set; }
        public string conversationID { get; set; }
    }
}
