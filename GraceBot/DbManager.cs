﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GraceBot.Models;
using Microsoft.Bot.Connector;
using System.Text.RegularExpressions;

namespace GraceBot
{
    public class DbManager : IDbManager
    {
        GraceBotContext _db;

        // constructor 
        public DbManager(GraceBotContext db)
        {
            _db = db;
        }
        
        // Implement the method defined in IDbManager interface.
        // Add an activity in database as an asynchronous operation when the activity is generated by the bot.
        public async Task AddActivity(Activity activity, ProcessStatus processStatus = ProcessStatus.BotMessage)
        {
            if (activity == null)
                throw new ArgumentNullException("activity cannot be null");
            if (activity.Id == null)
            {
                if (activity.ReplyToId == null)
                    throw new InvalidOperationException("Activity.Id and Activity.ReplyToId cannot both be null.");
                if (processStatus != ProcessStatus.BotMessage)
                    throw new InvalidOperationException("Activity.Id can be null only when processStatus equals to BotMessage");
            }

            var activityModel = ConvertToModel(activity, processStatus);
            AttachReference(activityModel);
            _db.Activities.Add(activityModel);
            try
            {
                await _db.SaveChangesAsync();
            } catch (Exception e)
            {
                var s = e.Message;
            }
        }

        public async Task UpdateActivityProcessStatus(string activityId, ProcessStatus processStatus)
        {
            if (activityId == null)
                throw new ArgumentNullException("activity cannot be null.");
            var oldRecord = _db.Activities.Include(r => r.From).Include(r => r.Recipient)
                .Include(r => r.Conversation).SingleOrDefault(o => o.ActivityId.Equals(activityId));            
            if(oldRecord != null)
            {
                oldRecord.ProcessStatus = processStatus;
                AttachReference(oldRecord);
                await _db.SaveChangesAsync();
            }
            else throw new RowNotInTableException("No matching Activity record is found.");
        }


        private void AttachReference(ActivityModel activityModel)
        {
            // Check if duplicate key in channelAccount and conversationAccount
            if (activityModel.From != null)
            {
                var channelAccountFrom = _db.ChannelAccounts.Find(activityModel.From.Id);
                if (channelAccountFrom != null)
                {
                    _db.ChannelAccounts.Attach(channelAccountFrom);
                    activityModel.From = channelAccountFrom;
                }
            }

            if (activityModel.Recipient != null)
            {
                var channelRecipient = _db.ChannelAccounts.Find(activityModel.Recipient.Id);
                if (channelRecipient != null)
                {
                    _db.ChannelAccounts.Attach(channelRecipient);
                    activityModel.Recipient = channelRecipient;
                }
            }

            if (activityModel.Conversation != null)
            {
                var conversationAccount = _db.ConversationAccounts.Find(activityModel.Conversation.Id);
                if (conversationAccount != null)
                {
                    _db.ConversationAccounts.Attach(conversationAccount);
                    activityModel.Conversation = conversationAccount;
                }
            }
        }


        // Implement the method defined in IDbManager interface.
        // Return a list of activities in database (5 contiguous ones from the start) which stand for unprocessed questions.
        public List<Activity> FindUnprocessedQuestions(int amount = 5, List<string> keywords = null)
        {
            if (amount < 1)
                throw new ArgumentOutOfRangeException("amount cannot be less than 1.");

            var query = _db.Activities
                            .Include(r => r.From)
                            .Include(r => r.Recipient)
                            .Include(r => r.Conversation)
                            .Where(o => o.ProcessStatus == ProcessStatus.Unprocessed);

            if (keywords != null && keywords.Any())
            {
                List<string> variants = new List<string>();
                foreach (var w in keywords)
                {
                    variants.Add(w.ToLower());
                    variants.Add(w.ToUpper());
                    variants.Add(w.ToLowerInvariant());
                    variants.Add(w.ToUpperInvariant());
                }
                keywords.AddRange(variants);
                query = query.Where(o => keywords.Any(w => o.Text.Contains(w)));
            }

            var records = query.Take(amount).ToList();
            var activities = new List<Activity>();
            foreach (var am in records)
            {
                activities.Add(ConvertToActivity(am));
            }
            return activities;
        }


        // Implement the method defined in IDbManager interface.
        // Return an activity (if found in database) given the ID.
        public Activity FindActivity(string id)
        {
            var activityRecord = _db.Activities.Include(r => r.From).Include(r => r.Recipient)
                .Include(r => r.Conversation).FirstOrDefault(o => o.ActivityId == id);
            if (activityRecord == null) return null;
            return ConvertToActivity(activityRecord);
        }

        public UserRole GetUserRole(string channelAccountId)
        {
            if (channelAccountId == null)
                throw new ArgumentNullException("channelAccountId cannot be null.");

            var channelAccount = _db.ChannelAccounts.Find(channelAccountId);
            if (channelAccount == null)
                throw new RowNotInTableException("ChannelAccount is not found.");
            if (channelAccount.UserAccountId == null)
                throw new RowNotInTableException("This ChannelAccount does not belong to any UserAccount");
            var userAccount = _db.UserAccounts.Find(channelAccount.UserAccountId);
            if (userAccount == null)
                throw new RowNotInTableException(
                    $"The UserAccount (Id: {channelAccount.UserAccountId}) is not in database, referential integrity might be broken.");
            return userAccount.Role;
        }

        // Return an activityModel given an activity and its process status.
        internal static ActivityModel ConvertToModel(Activity activity, ProcessStatus? processStatus = null)
        {
            var model = new ActivityModel(activity);
            if (processStatus != null)
                model.ProcessStatus = (ProcessStatus)processStatus;
            return model;
        }

        // Return an activity given an activityModel.
        internal static Activity ConvertToActivity(ActivityModel activityModel)
        {
            var from = new ChannelAccount()
            {
                Id = activityModel.From.Id,
                Name = activityModel.From.Name
            };

            var recipient = new ChannelAccount()
            {
                Id = activityModel.Recipient.Id,
                Name = activityModel.Recipient.Name
            };

            var conversation = new ConversationAccount()
            {
                Id = activityModel.Conversation.Id,
                IsGroup = activityModel.Conversation.IsGroup,
                Name = activityModel.Conversation.Name
            };

            return new Activity()
            {
                Id = activityModel.ActivityId,
                Text = activityModel.Text,
                Type = activityModel.Type,
                ServiceUrl = activityModel.ServiceUrl,
                Timestamp = activityModel.Timestamp,
                ChannelId = activityModel.ChannelId,
                From = from,
                Conversation = conversation,
                Recipient = recipient,
                ReplyToId = activityModel.ReplyToId
            };
        }
    }
}