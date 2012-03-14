﻿using System;
using System.Xml;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Fields.Settings;
using Orchard.Fields.ViewModels;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Orchard.Fields.Drivers {
    [UsedImplicitly]
    public class DateTimeFieldDriver : ContentFieldDriver<Fields.DateTimeField> {
        public IOrchardServices Services { get; set; }
        private const string TemplateName = "Fields/DateTime.Edit"; // EditorTemplates/Fields/DateTime.Edit.cshtml

        public DateTimeFieldDriver(IOrchardServices services) {
            Services = services;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        private static string GetPrefix(ContentField field, ContentPart part) {
            return part.PartDefinition.Name + "." + field.Name;
        }

        protected override DriverResult Display(ContentPart part, Fields.DateTimeField field, string displayType, dynamic shapeHelper) {
            return ContentShape("Fields_DateTime", // this is just a key in the Shape Table
                () => {
                    var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                    var value = field.DateTime;

                    var viewModel = new DateTimeFieldViewModel {
                        Name = field.DisplayName,
                        Date = value != DateTime.MinValue ? value.ToLocalTime().ToShortDateString() : String.Empty,
                        Time = value != DateTime.MinValue ? value.ToLocalTime().ToShortTimeString() : String.Empty,
                        ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                        ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly,
                        Hint = settings.Hint,
                        Required = settings.Required
                    };

                    return shapeHelper.Fields_DateTime( // this is the actual Shape which will be resolved (Fields/DateTime.cshtml)
                        Model: viewModel);
                }
            );
        }

        protected override DriverResult Editor(ContentPart part, Fields.DateTimeField field, dynamic shapeHelper) {
           
            var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
            var value = field.DateTime;

            if (value != DateTime.MinValue) {
                value = value.ToLocalTime();
            }
            
            var viewModel = new DateTimeFieldViewModel {
                Name = field.DisplayName,
                Date = value != DateTime.MinValue ? value.ToLocalTime().ToShortDateString() : "",
                Time = value != DateTime.MinValue ? value.ToLocalTime().ToShortTimeString() : "",
                ShowDate = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.DateOnly,
                ShowTime = settings.Display == DateTimeFieldDisplays.DateAndTime || settings.Display == DateTimeFieldDisplays.TimeOnly,
                Hint = settings.Hint,
                Required = settings.Required
            };

            return ContentShape("Fields_DateTime_Edit", // this is just a key in the Shape Table
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: viewModel, Prefix: GetPrefix(field, part))); 
        }

        protected override DriverResult Editor(ContentPart part, Fields.DateTimeField field, IUpdateModel updater, dynamic shapeHelper) {
            var viewModel = new DateTimeFieldViewModel();

            if(updater.TryUpdateModel(viewModel, GetPrefix(field, part), null, null)) {
                DateTime value;

                var settings = field.PartFieldDefinition.Settings.GetModel<DateTimeFieldSettings>();
                
                if(settings.Required && (String.IsNullOrWhiteSpace(viewModel.Time) || String.IsNullOrWhiteSpace(viewModel.Date))) {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is required", field.DisplayName));
                }

                if ( settings.Display == DateTimeFieldDisplays.DateOnly ) {
                    viewModel.Time = "00:00:00";
                }

                if ( settings.Display == DateTimeFieldDisplays.TimeOnly ) {
                    viewModel.Date = "1980-01-01";
                }

                if ( DateTime.TryParse(viewModel.Date + " " + viewModel.Time, out value) ) {
                    field.DateTime = value.ToUniversalTime();
                }
                else {
                    updater.AddModelError(GetPrefix(field, part), T("{0} is an invalid date and time", field.DisplayName));
                    field.DateTime = DateTime.MinValue;
                }
            }
            
            return Editor(part, field, shapeHelper);
        }

        protected override void Importing(ContentPart part, Fields.DateTimeField field, ImportContentContext context) {
            context.ImportAttribute(GetPrefix(field, part), "Value", v => field.Storage.Set(null, XmlConvert.ToDateTime(v, XmlDateTimeSerializationMode.Utc)));
        }

        protected override void Exporting(ContentPart part, Fields.DateTimeField field, ExportContentContext context) {
            context.Element(GetPrefix(field, part)).SetAttributeValue("DateTime", XmlConvert.ToString(field.Storage.Get<DateTime>(null), XmlDateTimeSerializationMode.Utc));
        }

        protected override void Describe(DescribeMembersContext context) {
            context
                .Member(null, typeof(DateTime), T("Value"), T("The date time value of the field."));
        }
    }
}
