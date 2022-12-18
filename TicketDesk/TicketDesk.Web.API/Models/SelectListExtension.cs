using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TicketDesk.Web.API.Models
{
    public static class SelectListExtension
    {
        private static string DefaultItemText { get { return TicketDesk.Localization.Models.Strings.SelectList_DefaultItem; } }

        public static SelectList ToSelectList<TSource, TValue>
       (
           this IEnumerable<TSource> items,
           Func<TSource, TValue> valueField,
           Func<TSource, string> textField,
           object selectedValue = null,
           bool includeDefaultItem = true
       ) where TValue : class
        {
            var list = items.Select(i => new SelectListQueryItem<TValue> { Id = valueField(i), Name = textField(i) }).ToList();
            AddDefaultItem(list, includeDefaultItem);
            return new SelectList(list, "ID", "Name", selectedValue);
        }

        #region utility

        internal class SelectListQueryItem<T>
        {
            public string Name { get; set; }
            public T Id { get; set; }
        }

        private static void AddDefaultItem<T>(IList<SelectListQueryItem<T>> list, bool includeDefaultItem)
        {
            if (includeDefaultItem)
            {
                list.Insert(0, GetDefaultSelectListItem<T>());
            }
        }

        private static SelectListQueryItem<T> GetDefaultSelectListItem<T>()
        {
            return new SelectListQueryItem<T> { Name = DefaultItemText, Id = default(T) };
        }
        #endregion



    }
}