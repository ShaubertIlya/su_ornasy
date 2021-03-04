using Devir.DMS.DL.Models.Document.NegotiatorsRoutes;
using Devir.DMS.Web.Models.Reference;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Devir.DMS.Web.HtmlHelpers
{
    public static class DevirEditorsHelper
    {
        //public static string BlueTextBox(this HtmlHelper helper)
        //{
        //    var sb = new StringBuilder();
        //    var sw = new StringWriter(sb);
        //    var htw = new HtmlTextWriter(sw);

        //    var tb = new TextBox();
        //    tb.BackColor = System.Drawing.Color.Blue;

        //    tb.RenderControl(htw);

        //    return sb.ToString();    
        //}

        public static MvcHtmlString DevirEditorFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, TModel model, bool isEdit) where TModel : DynamicRecordFieldViewModel
        {
            //MvcHtmlString result = new MvcHtmlString(String.Empty);

            //var model = (TModel)ModelMetadata.FromLambdaExpression(expression, html.ViewData).Model;

            //var h2 = html.ViewContext.ViewData.TemplateInfo;

            var h1 = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
            



            

            if (model.TypeOfTheFieldId.ToString() == "a427dbfb-9cb7-4f52-9d5e-c7d0677e8103" || model.TypeOfTheFieldId.ToString() == "f23165db-7c3d-49d5-bbc0-127eef90de36")
                return EditorExtensions.EditorFor(html, expression);

            if (model.TypeOfTheFieldId.ToString() == "2490becb-3476-43ab-8717-0f0b138a6ab2")
            {
                bool res = false;
                Boolean.TryParse(model.Value, out res);
                return html.CheckBox("Value", res);
                //html.RenderPartial("EditorTemplates/BoolEditorTemplate", new Tuple<Boolean,string>(res, "Value"));              
            }

            if (model.TypeOfTheFieldId.ToString() == "8a37142c-0e29-4b40-b4a3-0a3a7d4f21d9")
            {
                return EditorExtensions.EditorFor(html, expression);
            }

            if (model.TypeOfTheFieldId.ToString() == "944388a1-b1e3-4a4d-910d-7ad9df107e20")
            {
                return EditorExtensions.EditorFor(html, expression);
            }

            if (model.TypeOfTheFieldId.ToString() == "d88f464a-ca95-4c41-ad7d-7df5adfd90d8")
            {
                DateTime res = DateTime.Now;
                DateTime.TryParse(model.Value, out res);
                html.RenderPartial("EditorTemplates/DateTimeEditorTemplate", new Tuple<DateTime, string>(res, h1)); 
            }

            if (model.TypeOfTheFieldId.ToString() == "9b7be5b0-3e69-466a-a876-eae0402ebbe7")
            {
                html.RenderPartial("EditorTemplates/UserSelectorEditorTemplate", new DynamicReferenceEditorTemplateViewModel() { ReferenceId = Guid.Empty, SelectedItemGuid = model.ValueId, FieldName = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix, FieldStringValue = model.Value, FieldStringHeader = Guid.NewGuid().ToString() });
            }

            if (model.DynamicReferenceId != Guid.Empty)
            {
                Guid SpravochnikRes;
                Guid.TryParse(model.Value, out SpravochnikRes);
                if (isEdit == false)
                    html.RenderPartial("EditorTemplates/DynamicReferenceEditorTemplate", new DynamicReferenceEditorTemplateViewModel() { ReferenceId = model.DynamicReferenceId, SelectedItemGuid = SpravochnikRes, FieldName = h1, FieldStringValue = model.DynamicReferenceResult, FieldStringHeader = Guid.NewGuid().ToString() });
                else
                    html.RenderPartial("EditorTemplates/DynamicReferenceEditorTemplate", new DynamicReferenceEditorTemplateViewModel() { ReferenceId = model.DynamicReferenceId, SelectedItemGuid = model.ValueId, FieldName = h1, FieldStringValue = model.Value, FieldStringHeader = Guid.NewGuid().ToString() });
            }

            if (model.TypeOfTheFieldId.ToString() == "e3224442-d53a-47e9-b1bb-495c034b10d8")
            {
               // return html.EditorFor(m => m.Negotiators);
                //html.RenderPartial("EditorTemplates/NegotiatorTemplate", model.Negotiators);
                html.RenderAction("Index", "NegotiatorsKO", new { model=model.ModelHelper });
            }

            if (model.TypeOfTheFieldId.ToString() == "2c308153-04a9-4bf6-b021-cc28b82a7ab5")
            {
               // return html.EditorFor(m => m.Negotiators);
                //html.RenderPartial("EditorTemplates/NegotiatorTemplate", model.Negotiators);
                html.RenderAction("Index", "UsersKO", new { model = model.ValueUsersKo, FieldName = model.Header, FieldPath = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix });
            }
            
            return null;

           
            

            
            //result = EditorExtensions.EditorFor(html, expression, "сюда шаблон!");

        }
    }
}