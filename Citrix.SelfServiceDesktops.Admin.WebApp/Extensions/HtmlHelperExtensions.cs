/*
 * Copyright (c) 2013 Citrix Systems, Inc. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Citrix.SelfServiceDesktops.Admin.WebApp.Extensions {

    public static partial class HtmlHelperExtensions {

        public static MvcHtmlString LabelWithToolTipFor<TModel, TValue>
            (this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression,
            string labelText) 
        {
   
            return helper.LabelFor(expression, labelText, CreateDictionary(helper, expression));
        }

        public static MvcHtmlString LabelWithToolTipFor<TModel, TValue>
            (this HtmlHelper<TModel> helper,
            Expression<Func<TModel, TValue>> expression) 
        {

            return helper.LabelFor (expression, CreateDictionary(helper, expression));
        }

        private static ViewDataDictionary CreateDictionary<TModel, TValue>(
            this HtmlHelper<TModel> helper,
             Expression<Func<TModel, TValue>> expression) {
            if (expression == null) {
                throw new ArgumentNullException("expression");
            }
            ViewDataDictionary dictionary = new ViewDataDictionary();
            var metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            if (!string.IsNullOrEmpty(metaData.Description)) {
                ResourceManager rm = new ResourceManager(typeof(Resources.HelpText));
                string description = rm.GetString(metaData.Description);
                if (description == null) {
                    dictionary.Add("title", metaData.Description);
                } else {
                    dictionary.Add("title", description);
                }
            }
            return dictionary;
        }

    }
}