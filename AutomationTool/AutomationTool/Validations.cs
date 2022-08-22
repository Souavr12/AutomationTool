/* -------------------------------------------------------------------------------------------------
   Restricted - Copyright (C) Siemens Healthcare GmbH/Siemens Medical Solutions USA, Inc., 2020. All rights reserved
   ------------------------------------------------------------------------------------------------- */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using AutomationTool.Extension;

namespace AutomationTool.Validate
{
    public enum CompareFigures
    {
        GreaterThan = 0,
        LessThan = 1,
        Equals = 2,
        GreaterThanEquals = 3,
        LessThanEquals = 4
    }

    [DisplayName("Validations")]
    public static class Validations
    {
        /// <summary>
        /// Validates the control if it is required.
        /// </summary>
        /// <typeparam name="T">Type Object</typeparam>
        /// <param name="RequiredFields">Required Control to validate</param>
        /// <param name="ValidationMessage">Message if validation fails</param>
        /// <returns>Miscellaneous result</returns>

        [DisplayName("Required")]
        public static Tuple<bool, string> Required<T>(this T RequiredFields, string ValidationMessage) where T : System.Windows.Forms.Control
        {
            Tuple<bool, string> rtnValue = Tuple.Create<bool, string>(true, string.Empty);
            try
            {
                if (RequiredFields != null)
                {
                    string ClassName = typeof(T).Name;
                    switch (ClassName.ToLower())
                    {
                        case "textbox":
                            System.Windows.Forms.TextBox TBox = RequiredFields as System.Windows.Forms.TextBox;
                            if (TBox != null)
                            {
                                if (string.IsNullOrEmpty(TBox.Text.Trim()))
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        case "combobox":
                            System.Windows.Forms.ComboBox CBox = RequiredFields as System.Windows.Forms.ComboBox;
                            if (CBox != null)
                            {
                                if (CBox.SelectedIndex <= 0)
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        case "datagridview":
                            System.Windows.Forms.DataGridView DataGrid = RequiredFields as System.Windows.Forms.DataGridView;
                            if (DataGrid != null)
                            {
                                if (DataGrid.SelectedRows.Count == 0)
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        case "checkbox":
                            System.Windows.Forms.CheckBox CHKBox = RequiredFields as System.Windows.Forms.CheckBox;
                            if (CHKBox != null)
                            {
                                if (CHKBox.Checked)
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        case "maskedtextbox":
                            System.Windows.Forms.MaskedTextBox MBox = RequiredFields as System.Windows.Forms.MaskedTextBox;
                            if (MBox != null)
                            {
                                if (string.IsNullOrEmpty(System.Text.RegularExpressions.Regex.Replace(MBox.Text.Trim(), @"[^\w\d]", "").Trim()))
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        case "richtextbox":
                            System.Windows.Forms.RichTextBox RTextBox = RequiredFields as System.Windows.Forms.RichTextBox;
                            if (RTextBox != null)
                            {
                                if (string.IsNullOrEmpty(RTextBox.Text.Trim()))
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        case "checkedlistbox":
                            System.Windows.Forms.CheckedListBox CListBox = RequiredFields as System.Windows.Forms.CheckedListBox;
                            if (CListBox != null)
                            {
                                if (CListBox.CheckedItems.Count == 0)
                                {
                                    rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                    return rtnValue;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return rtnValue;
        }

        /// <summary>
        /// Validates the control if it is a range to validate.
        /// </summary>
        /// <typeparam name="T">Type Object</typeparam>
        /// <param name="RequiredFields">Required Control to validate</param>
        /// <param name="ValidationMessage">Message if validation fails</param>
        /// <param name="MinValue">Minimum Value</param>
        /// <param name="MaxValue">Maximum Value</param>
        /// <returns>Miscellaneous result</returns>
        [DisplayName("Range")]
        public static Tuple<bool, string> Range<T>(this T RequiredFields, string ValidationMessage, dynamic MinValue, dynamic MaxValue) where T : System.Windows.Forms.Control
        {
            Tuple<bool, string> rtnValue = Tuple.Create<bool, string>(true, string.Empty);
            dynamic value = default(dynamic);
            try
            {
                if (RequiredFields != null)
                {
                    string ClassName = typeof(T).Name;
                    switch (ClassName.ToLower())
                    {
                        case "textbox":
                            System.Windows.Forms.TextBox TBox = RequiredFields as System.Windows.Forms.TextBox;
                            if (TBox != null)
                            {
                                if (!string.IsNullOrEmpty(TBox.Text.Trim()))
                                {
                                    if (Microsoft.VisualBasic.Information.IsNumeric(TBox.Text.Trim()))
                                        value = TBox.Text.Trim();
                                    else if (Microsoft.VisualBasic.Information.IsDate(TBox.Text.Trim()))
                                        value = TBox.Text.Trim();
                                    else
                                        value = default(dynamic);

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsNumeric(MinValue) && Microsoft.VisualBasic.Information.IsNumeric(MaxValue))
                                        {
                                            if (!(Convert.ToInt32(value) >= Convert.ToInt32(MinValue) && Convert.ToInt32(value) <= Convert.ToInt32(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                        else if (Microsoft.VisualBasic.Information.IsDate(MinValue) && Microsoft.VisualBasic.Information.IsDate(MaxValue))
                                        {
                                            if (!(Convert.ToDateTime(value) >= Convert.ToDateTime(MinValue) && Convert.ToDateTime(value) <= Convert.ToDateTime(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "maskedtextbox":
                            System.Windows.Forms.MaskedTextBox MBox = RequiredFields as System.Windows.Forms.MaskedTextBox;
                            if (MBox != null)
                            {
                                if (!string.IsNullOrEmpty(System.Text.RegularExpressions.Regex.Replace(MBox.Text.Trim(), @"[^\w\d]", "").Trim()))
                                {
                                    if (Microsoft.VisualBasic.Information.IsNumeric(MBox.Text.Trim()))
                                        value = MBox.Text.Trim();
                                    else if (Microsoft.VisualBasic.Information.IsDate(MBox.Text.Trim()))
                                        value = MBox.Text.Trim();
                                    else
                                        value = default(dynamic);

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsNumeric(MinValue) && Microsoft.VisualBasic.Information.IsNumeric(MaxValue))
                                        {
                                            if (!(Convert.ToInt32(value) >= Convert.ToInt32(MinValue) && Convert.ToInt32(value) <= Convert.ToInt32(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                        else if (Microsoft.VisualBasic.Information.IsDate(MinValue) && Microsoft.VisualBasic.Information.IsDate(MaxValue))
                                        {
                                            if (!(Convert.ToDateTime(value) >= Convert.ToDateTime(MinValue) && Convert.ToDateTime(value) <= Convert.ToDateTime(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "combobox":
                            System.Windows.Forms.ComboBox CBox = RequiredFields as System.Windows.Forms.ComboBox;
                            if (CBox != null)
                            {
                                if (CBox.SelectedIndex > 0)
                                {
                                    if (Microsoft.VisualBasic.Information.IsNumeric(CBox.Text.Trim()))
                                        value = CBox.Text.Trim();
                                    else if (Microsoft.VisualBasic.Information.IsDate(CBox.Text.Trim()))
                                        value = CBox.Text.Trim();
                                    else
                                        value = default(dynamic);

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsNumeric(MinValue) && Microsoft.VisualBasic.Information.IsNumeric(MaxValue))
                                        {
                                            if (!(Convert.ToInt32(value) >= Convert.ToInt32(MinValue) && Convert.ToInt32(value) <= Convert.ToInt32(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                        else if (Microsoft.VisualBasic.Information.IsDate(MinValue) && Microsoft.VisualBasic.Information.IsDate(MaxValue))
                                        {
                                            if (!(Convert.ToDateTime(value) >= Convert.ToDateTime(MinValue) && Convert.ToDateTime(value) <= Convert.ToDateTime(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        case "richtextbox":
                            System.Windows.Forms.RichTextBox RTextBox = RequiredFields as System.Windows.Forms.RichTextBox;
                            if (RTextBox != null)
                            {
                                if (!string.IsNullOrEmpty(RTextBox.Text.Trim()))
                                {
                                    if (Microsoft.VisualBasic.Information.IsNumeric(RTextBox.Text.Trim()))
                                        value = RTextBox.Text.Trim();
                                    else if (Microsoft.VisualBasic.Information.IsDate(RTextBox.Text.Trim()))
                                        value = RTextBox.Text.Trim();
                                    else
                                        value = default(dynamic);

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsNumeric(MinValue) && Microsoft.VisualBasic.Information.IsNumeric(MaxValue))
                                        {
                                            if (!(Convert.ToInt32(value) >= Convert.ToInt32(MinValue) && Convert.ToInt32(value) <= Convert.ToInt32(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                        else if (Microsoft.VisualBasic.Information.IsDate(MinValue) && Microsoft.VisualBasic.Information.IsDate(MaxValue))
                                        {
                                            if (!(Convert.ToDateTime(value) >= Convert.ToDateTime(MinValue) && Convert.ToDateTime(value) <= Convert.ToDateTime(MaxValue)))
                                            {
                                                rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                                return rtnValue;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return rtnValue;
        }

        /// <summary>
        /// Validates the control if there is a comparision available.
        /// </summary>
        /// <typeparam name="T">Type Object</typeparam>
        /// <param name="RequiredFields">Required Control to validate</param>
        /// <param name="ValidationMessage">Message if validation fails</param>
        /// <param name="CompareValue">Value to compare</param>
        /// <returns>Miscellaneous result</returns>
        [DisplayName("Compare")]
        public static Tuple<bool, string> Compare<T>(this T RequiredFields, string ValidationMessage, dynamic CompareValue, CompareFigures CompareType = CompareFigures.Equals) where T : System.Windows.Forms.Control
        {
            Tuple<bool, string> rtnValue = Tuple.Create<bool, string>(true, string.Empty);
            dynamic value = default(dynamic);
            try
            {
                if (RequiredFields != null)
                {
                    string ClassName = typeof(T).Name;
                    switch (ClassName.ToLower())
                    {
                        case "textbox":
                            System.Windows.Forms.TextBox TBox = RequiredFields as System.Windows.Forms.TextBox;
                            if (TBox != null)
                            {
                                if (!string.IsNullOrEmpty(TBox.Text.Trim()))
                                {
                                    if (Microsoft.VisualBasic.Information.IsNumeric(TBox.Text.Trim()))
                                        value = TBox.Text.Trim();
                                    else if (Microsoft.VisualBasic.Information.IsDate(TBox.Text.Trim()))
                                        value = TBox.Text.Trim();
                                    else
                                        value = default(dynamic);

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsDate(CompareValue))
                                            rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType, IsDate: true), ValidationMessage);
                                        else
                                            rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType), ValidationMessage);
                                    }
                                }
                            }
                            break;
                        case "datetimepicker":
                            System.Windows.Forms.DateTimePicker DtpBox = RequiredFields as System.Windows.Forms.DateTimePicker;
                            if (DtpBox != null)
                            {
                                if (!string.IsNullOrEmpty(DtpBox.Text.Trim()))
                                {
                                    value = DtpBox.Text.Trim();
                                    if (value != default(dynamic))
                                        rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType, IsDate: true), ValidationMessage);
                                }
                            }
                            break;
                        case "combobox":
                            System.Windows.Forms.ComboBox CBox = RequiredFields as System.Windows.Forms.ComboBox;
                            if (CBox != null)
                            {
                                if (!string.IsNullOrEmpty(CBox.Text.Trim()))
                                {
                                    value = CBox.Text.Trim();
                                    if (value != default(dynamic))
                                        rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType), ValidationMessage);
                                }
                            }
                            break;
                        case "maskedtextbox":
                            System.Windows.Forms.MaskedTextBox MBox = RequiredFields as System.Windows.Forms.MaskedTextBox;
                            if (MBox != null)
                            {
                                if (!string.IsNullOrEmpty(System.Text.RegularExpressions.Regex.Replace(MBox.Text.Trim(), @"[^\w\d]", "").Trim()))
                                {
                                    if (Microsoft.VisualBasic.Information.IsNumeric(MBox.Text.Trim()))
                                        value = MBox.Text.Trim();
                                    else if (Microsoft.VisualBasic.Information.IsDate(MBox.Text.Trim()))
                                        value = MBox.Text.Trim();
                                    else
                                        value = default(dynamic);

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsNumeric(CompareValue))
                                            rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType), ValidationMessage);

                                        if (Microsoft.VisualBasic.Information.IsDate(CompareValue))
                                            rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType, IsDate: true), ValidationMessage);
                                    }
                                }
                            }
                            break;
                        case "richtextbox":
                            System.Windows.Forms.RichTextBox RTextBox = RequiredFields as System.Windows.Forms.RichTextBox;
                            if (RTextBox != null)
                            {
                                if (!string.IsNullOrEmpty(RTextBox.Text.Trim()))
                                {
                                    value = RTextBox.Text.Trim();

                                    if (value != default(dynamic))
                                    {
                                        if (Microsoft.VisualBasic.Information.IsNumeric(CompareValue))
                                            rtnValue = Tuple.Create<bool, string>(CompareValues(value1: CompareValue, value2: value, CompareType: CompareType), ValidationMessage);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return rtnValue;
        }

        /// <summary>
        /// Validates the control if there is a expression available.
        /// </summary>
        /// <typeparam name="T">Type Object</typeparam>
        /// <param name="RequiredFields">Required Control to validate</param>
        /// <param name="ValidationMessage">Message if validation fails</param>
        /// <param name="Pattern">Regular expression to match</param>
        /// <returns>Miscellaneous result</returns>
        [DisplayName("Regular Expression")]
        public static Tuple<bool, string> RegularExpression<T>(this T RequiredFields, string ValidationMessage, string Pattern) where T : System.Windows.Forms.Control
        {
            Tuple<bool, string> rtnValue = Tuple.Create<bool, string>(true, string.Empty);
            try
            {
                if (RequiredFields != null)
                {
                    string ClassName = typeof(T).Name;
                    switch (ClassName.ToLower())
                    {
                        case "textbox":
                            System.Windows.Forms.TextBox TBox = RequiredFields as System.Windows.Forms.TextBox;
                            if (TBox != null)
                            {
                                if (!string.IsNullOrEmpty(TBox.Text.Trim()))
                                {
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(TBox.Text.Trim(), Pattern))
                                    {
                                        rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                        return rtnValue;
                                    }
                                }
                            }
                            break;
                        case "combobox":
                            System.Windows.Forms.ComboBox CBox = RequiredFields as System.Windows.Forms.ComboBox;
                            if (CBox != null)
                            {
                                if (!string.IsNullOrEmpty(CBox.Text.Trim()))
                                {
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(CBox.Text.Trim(), Pattern))
                                    {
                                        rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                        return rtnValue;
                                    }
                                }
                            }
                            break;
                        case "maskedtextbox":
                            System.Windows.Forms.MaskedTextBox MBox = RequiredFields as System.Windows.Forms.MaskedTextBox;
                            if (MBox != null)
                            {
                                if (!string.IsNullOrEmpty(System.Text.RegularExpressions.Regex.Replace(MBox.Text.Trim(), @"[^\w\d]", "").Trim()))
                                {
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(MBox.Text.Trim(), Pattern))
                                    {
                                        rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                        return rtnValue;
                                    }
                                }
                            }
                            break;
                        case "richtextbox":
                            System.Windows.Forms.RichTextBox RTextBox = RequiredFields as System.Windows.Forms.RichTextBox;
                            if (RTextBox != null)
                            {
                                if (!string.IsNullOrEmpty(RTextBox.Text.Trim()))
                                {
                                    if (!System.Text.RegularExpressions.Regex.IsMatch(RTextBox.Text.Trim(), Pattern))
                                    {
                                        rtnValue = Tuple.Create<bool, string>(false, ValidationMessage);
                                        return rtnValue;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return rtnValue;
        }

        /// <summary>
        /// Validates all properties.
        /// </summary>
        /// <typeparam name="T">Type parameter for Data Object</typeparam>
        /// <param name="DataObject">Object of type T</param>
        /// <returns>Tuple of Boolean and string</returns>
        [DisplayName("Validate Properties")]
        public static Tuple<bool, string> ValidateProperties<T>(this T DataObject)
        {
            Tuple<bool, string> rtnValue = null;
            string fields = "";
            try
            {
                var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(DataObject, null, null);
                var validationResult = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

                var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(DataObject, validationContext, validationResult, true);

                validationResult.ForEach(result => fields = fields + result.ErrorMessage + Environment.NewLine + "        ");
                string ErrorMessage = fields.Trim();
                rtnValue = Tuple.Create<bool, string>(isValid, ErrorMessage);
            }
            catch (Exception ex)
            {
                ex.LogError(MethodName: System.Reflection.MethodBase.GetCurrentMethod().Name, ClassName: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            }
            return rtnValue;
        }

        private static bool CompareValues(dynamic value1, CompareFigures CompareType, dynamic value2, bool IsDate = false)
        {
            bool rtnValue = true;
            try
            {
                switch (CompareType)
                {
                    case CompareFigures.GreaterThan:
                        if (!IsDate)
                            rtnValue = Convert.ToInt32(value1) > Convert.ToInt32(value2) ? true : false;

                        if (IsDate)
                            rtnValue = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Day, Convert.ToDateTime(value1).Date, Convert.ToDateTime(value2).Date) > 0 ? true : false;
                        break;
                    case CompareFigures.LessThan:
                        if (!IsDate)
                            rtnValue = Convert.ToInt32(value1) < Convert.ToInt32(value2) ? true : false;

                        if (IsDate)
                            rtnValue = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Day, Convert.ToDateTime(value1).Date, Convert.ToDateTime(value2).Date) < 0 ? true : false;
                        break;
                    case CompareFigures.Equals:
                        if (!IsDate)
                            rtnValue = Convert.ToInt32(value1) == Convert.ToInt32(value2) ? true : false;

                        if (IsDate)
                            rtnValue = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Day, Convert.ToDateTime(value1).Date, Convert.ToDateTime(value2).Date) == 0 ? true : false;
                        break;
                    case CompareFigures.GreaterThanEquals:
                        if (!IsDate)
                            rtnValue = Convert.ToInt32(value1) >= Convert.ToInt32(value2) ? true : false;

                        if (IsDate)
                            rtnValue = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Day, Convert.ToDateTime(value1).Date, Convert.ToDateTime(value2).Date) >= 0 ? true : false;
                        break;
                    case CompareFigures.LessThanEquals:
                        if (!IsDate)
                            rtnValue = Convert.ToInt32(value1) <= Convert.ToInt32(value2) ? true : false;

                        if (IsDate)
                            rtnValue = Microsoft.VisualBasic.DateAndTime.DateDiff(Microsoft.VisualBasic.DateInterval.Day, Convert.ToDateTime(value1).Date, Convert.ToDateTime(value2).Date) <= 0 ? true : false;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + Environment.NewLine + "Class Name: " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + Environment.NewLine + "Method Name: " + System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return rtnValue;
        }
    }
}




namespace Utility.Validate
{
    
}
