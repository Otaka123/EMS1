using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Application.Contracts.interfaces
{

    /// <summary>
    /// واجهة مخصصة لخدمة الترجمة توفر وظائف إضافية عن IStringLocalizer القياسية
    /// </summary>
    public interface ISharedMessageLocalizer : IStringLocalizer
    {
        /// <summary>
        /// إعادة تحميل جميع الترجمات من الملفات
        /// </summary>
        void ReloadTranslations();

        /// <summary>
        /// التحقق من وجود ترجمات لثقافة محددة
        /// </summary>
        /// <param name="cultureCode">كود الثقافة (مثل "en-US")</param>
        /// <returns>true إذا كانت الثقافة مدعومة</returns>
        bool CultureExists(string cultureCode);

        /// <summary>
        /// الحصول على جميع الثقافات المدعومة
        /// </summary>
        /// <returns>قائمة بأكواد الثقافات المتاحة</returns>
        IEnumerable<string> GetAvailableCultures();

        /// <summary>
        /// الحصول على ترجمة مع معالجة خاصة للقيم الفارغة
        /// </summary>
        /// <param name="key">مفتاح الترجمة</param>
        /// <param name="defaultValue">القيمة الافتراضية إذا لم توجد الترجمة</param>
        /// <returns>النص المترجم أو القيمة الافتراضية</returns>
        string GetTranslation(string key, string? defaultValue = null);

        /// <summary>
        /// الحصول على ترجمة مع معلمات وتنسيق
        /// </summary>
        /// <param name="key">مفتاح الترجمة</param>
        /// <param name="defaultValue">القيمة الافتراضية إذا لم توجد الترجمة</param>
        /// <param name="args">معلمات التنسيق</param>
        /// <returns>النص المترجم أو القيمة الافتراضية</returns>
        string GetTranslation(string key, string defaultValue, params object[] args);
    }

}
