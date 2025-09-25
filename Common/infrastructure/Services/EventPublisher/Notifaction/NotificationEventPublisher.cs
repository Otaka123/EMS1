//using Common.Application.Common;
//using Common.Application.Common.Enums;
//using Common.Application.Contracts.interfaces;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Common.infrastructure.Services.EventPublisher.Notifaction
//{
//    public class NotificationEventPublisher : EventPublisher, IEventPublisher
//    {
//        private readonly ILogger<NotificationEventPublisher> _logger;
//        private readonly IEventPublisher _templateService;
//        //private readonly IEmailService _emailService;

//        public NotificationEventPublisher(
//            ILogger<NotificationEventPublisher> logger,
//            IEventPublisher templateService,
//            int maxQueueSize = 1000)
//            : base(logger, maxQueueSize)
//        {
//            _logger = logger;
//            _templateService = templateService;
//        }
//        public async Task ProcessNotificationAsync(NotificationEvent notification)
//        {
//            try
//            {
//                var tasks = new List<Task>();

//                if (notification.Channels.Contains(NotificationChannel.Email))
//                {
//                    tasks.Add(SendEmailNotification(notification));
//                }

//                if (notification.Channels.Contains(NotificationChannel.PushNotification))
//                {
//                    tasks.Add(SendPushNotification(notification));
//                }

//                await Task.WhenAll(tasks);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to process notification {notification.EventId}");
//                throw;
//            }
//        }

//        //private async Task SendEmailNotification(NotificationEvent notification)
//        //{
//        //    var emailContent = await _emailService.RenderTemplateAsync(
//        //        notification.TemplateId,
//        //        notification.Data);

//        //    await _emailService.SendAsync(
//        //        to: notification.RecipientId,
//        //        subject: emailContent.Subject,
//        //        body: emailContent.Body);
//        //}

//        //private async Task SendPushNotification(NotificationEvent notification)
//        //{
//        //    var message = await _pushService.CreateMessageAsync(
//        //        notification.TemplateId,
//        //        notification.Data);

//        //    await _pushService.SendAsync(
//        //        userId: notification.RecipientId,
//        //        message: message);
//        //}
//        public async Task<bool> PublishNotificationAsync(NotificationEvent notification)
//        {
//            try
//            {
//                // تحميل القالب إذا لم يكن محددًا
//                if (string.IsNullOrEmpty(notification.TemplateId))
//                {
//                    notification.TemplateId = await _templateService.GetDefaultTemplateIdAsync(notification.EventType);
//                }

//                return await PublishAsync(notification.EventType, notification);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to publish notification event {notification.EventId}");
//                return false;
//            }
//        }

//        public void SubscribeForNotification(string eventType, Func<NotificationEvent, Task> handler)
//        {
//            Subscribe(eventType, async (eventMsg) =>
//            {
//                var notification = JsonConvert.DeserializeObject<NotificationEvent>(eventMsg.Payload.ToString());
//                await handler(notification);
//            });
//        }
//    }
//}
