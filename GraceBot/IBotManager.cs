﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace GraceBot
{
    public interface IBotManager
    {
        /// <summary>
        /// Respond to an activity as an asynchronous operation.
        /// </summary>
        /// <param name="replyText">The text as a reply.</param>
        /// <param name="originalAcitivty">The activity to reply to.</param>
        /// <param name="attachments">The attachments for reply activity.</param>
        /// <returns></returns>
        Task<Activity> ReplyToActivityAsync(string replyText, Activity originalAcitivty,List<Attachment> attachments=null);
        /// <summary>
        /// Get user state as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="activity">The activity generated by the user.</param>
        /// <returns></returns>
        Task<T> GetUserDataPropertyAsync<T>(string property, Activity activity);

        /// <summary>
        /// Update user state as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="data"></param>
        /// <param name="activity">The activity generated by the user.</param>
        /// <returns></returns>
        Task SetUserDataPropertyAsync<T>(string property, T data, Activity activity);

        /// <summary>
        /// Delete user state as an asynchronous operation.
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        Task<string[]> DeleteStateForUserAsync(Activity activity);
        List<Attachment> GenerateQuestionsAttachments(List<Activity> activityList);
    }
}