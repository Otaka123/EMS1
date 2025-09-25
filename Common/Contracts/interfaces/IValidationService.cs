//using Common.Application.Common;
//using FluentValidation.Results;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Common.Application.Contracts.interfaces
//{
//    //public interface IValidationService
//    //{
//    //    Task<ValidationResult> ValidateAsync<T>(
//    //        T instance,
//    //        CancellationToken cancellationToken = default);
//    //    Task<bool> IsValidAsync<T>(
//    //       T instance,
//    //       CancellationToken cancellationToken = default);
//    //    //Task<bool> IsValidAsync<T>(
//    //    //    T instance,
//    //    //    IEnumerable<string> propertyNames,
//    //    //    CancellationToken cancellationToken = default);
//    //    Task<IEnumerable<string>> GetValidationMessagesAsync<T>(
//    //        T instance,
//    //        CancellationToken cancellationToken = default);
//    //}
//    /// <summary>
//    /// واجهة خدمة التحقق من صحة النماذج
//    /// </summary>
//    public interface IValidationService
//    {
//        /// <summary>
//        /// تحقق من صحة النموذج مع إرجاع ResponseError عند الفشل
//        /// </summary>
//        Task<ValidationResult> ValidateAsync<T>(
//            T instance,
//            CancellationToken cancellationToken = default) where T : class;

//        /// <summary>
//        /// تحقق مما إذا كان النموذج صالحًا
//        /// </summary>
//        /// <typeparam name="T">نوع النموذج</typeparam>
//        /// <param name="instance">النموذج المراد التحقق منه</param>
//        /// <param name="cancellationToken">رمز الإلغاء</param>
//        /// <returns>true إذا كان النموذج صالحًا، وإلا false</returns>
//        Task<bool> IsValidAsync<T>(
//            T instance,
//            CancellationToken cancellationToken = default) where T : class;

//        /// <summary>
//        /// الحصول على رسائل التحقق من الصحة
//        /// </summary>
//        /// <typeparam name="T">نوع النموذج</typeparam>
//        /// <param name="instance">النموذج المراد التحقق منه</param>
//        /// <param name="cancellationToken">رمز الإلغاء</param>
//        /// <returns>قائمة برسائل التحقق</returns>
//        Task<IEnumerable<string>> GetValidationMessagesAsync<T>(
//            T instance,
//            CancellationToken cancellationToken = default) where T : class;

//        /// <summary>
//        /// التحقق من صحة النموذج مع رمي استثناء إذا كان غير صالح
//        /// </summary>
//        /// <typeparam name="T">نوع النموذج</typeparam>
//        /// <param name="instance">النموذج المراد التحقق منه</param>
//        /// <param name="cancellationToken">رمز الإلغاء</param>
//        /// <exception cref="ValidationException">يتم رميه إذا كان النموذج غير صالح</exception>
//        Task EnsureValidAsync<T>(
//            T instance,
//            CancellationToken cancellationToken = default) where T : class;

//        /// <summary>
//        /// تحقق من صحة خصائص محددة في النموذج
//        /// </summary>
//        /// <typeparam name="T">نوع النموذج</typeparam>
//        /// <param name="instance">النموذج المراد التحقق منه</param>
//        /// <param name="propertyNames">أسماء الخصائص المراد التحقق منها</param>
//        /// <param name="cancellationToken">رمز الإلغاء</param>
//        /// <returns>نتيجة التحقق</returns>
//        Task<ValidationResult> ValidatePropertiesAsync<T>(
//            T instance,
//            IEnumerable<string> propertyNames,
//            CancellationToken cancellationToken = default) where T : class;
//        /// <summary>
//        /// إنشاء ResponseError من أخطاء التحقق
//        /// </summary>
//        ResponseError CreateValidationError(IEnumerable<ValidationFailure> failures);
//    }
//}
using Common.Application.Common;
using FluentValidation.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Application.Contracts.Interfaces
{
    /// <summary>
    /// واجهة خدمة التحقق من صحة النماذج
    /// </summary>
    public interface IValidationService
    {
        Task<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class;
        ResponseError CreateValidationError(IEnumerable<ValidationFailure> failures);
        Task<bool> IsValidAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<string>> GetValidationMessagesAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class;
        Task EnsureValidAsync<T>(T instance, CancellationToken cancellationToken = default) where T : class;
        Task<ValidationResult> ValidatePropertiesAsync<T>(T instance, IEnumerable<string> propertyNames, CancellationToken cancellationToken = default) where T : class;
    }
}