using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace PSC.RazorLibrary.Components.Helpers.Quill
{
    public static class QuillInterop
    {
        private const string strCreateQuill = "QuillFunctions.createQuill";
        private const string strGetText = "QuillFunctions.getQuillText";
        private const string strSetQuillText = "QuillFunctions.setQuillText";
        private const string strGetHTML = "QuillFunctions.getQuillHTML";
        private const string strGetContent = "QuillFunctions.getQuillContent";
        private const string strGetQuillSelection = "QuillFunctions.getQuillSelection";
        private const string strSetQuillSelection = "QuillFunctions.setQuillSelection";
        private const string strQuillFocus = "QuillFunctions.quillFocus";
        private const string strInsertQuillText = "QuillFunctions.insertQuillText";
        private const string strLoadQuillContent = "QuillFunctions.loadQuillContent";
        private const string strEnableQuillEditor = "QuillFunctions.enableQuillEditor";
        private const string strGetQuillEmbedded = "QuillFunctions.getQuillEmbedded";
        private const string strCreateQuillEmbedded = "QuillFunctions.createQuillEmbedded";
        private const string strClearQuillEmbedded = "QuillFunctions.clearQuillEmbedded";
        private const string strGetQuillAttachedFiles = "QuillFunctions.getQuillAttachedFiles";

        internal static ValueTask<object> CreateQuill(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            ElementReference toolbar,
            bool hasToolBar,
            bool readOnly,
            string placeholder,
            string theme,
            string debugLevel)
        {
            return jsRuntime.InvokeAsync<object>(
                strCreateQuill,
                quillElement, toolbar, hasToolBar, readOnly,
                placeholder, theme, debugLevel);
        }

        internal static ValueTask<string> GetText(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(
                strGetText,
                quillElement);
        }

        internal static ValueTask SetQuillText(
            IJSRuntime jSRuntime,
            ElementReference quillElement,
            string Content)
        {
            return jSRuntime.InvokeVoidAsync(strSetQuillText, quillElement, Content);
        } 

        internal static ValueTask<string> GetHTML(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(
                strGetHTML,
                quillElement);
        }

        internal static ValueTask<string> GetContent(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(
                strGetContent,
                quillElement);
        }

        internal static ValueTask<object> LoadQuillContent(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            string Content)
        {
            return jsRuntime.InvokeAsync<object>(
                strLoadQuillContent,
                quillElement, Content);
        }

        internal static ValueTask<string> GetQuillSelection(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<string>(strGetQuillSelection, quillElement);
        }

        internal static ValueTask<object> SetQuillSelection(
            IJSRuntime jSRuntime,
            ElementReference quillElement,
            int index,
            int length)
        {
            return jSRuntime.InvokeAsync<object>(strSetQuillSelection, quillElement, index, length);
        }

        internal static ValueTask<object> QuillFocus(
            IJSRuntime jsRuntime,
            ElementReference quillElement)
        {
            return jsRuntime.InvokeAsync<object>(strQuillFocus, quillElement);
        }

        internal static ValueTask<object> InsertQuillText(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            int Index,
            string Content)
        {
            return jsRuntime.InvokeAsync<object>(strInsertQuillText, quillElement, Index, Content);
        }

        internal static ValueTask<object> EnableQuillEditor(
            IJSRuntime jsRuntime,
            ElementReference quillElement,
            bool mode)
        {
            return jsRuntime.InvokeAsync<object>(
                strEnableQuillEditor,
                quillElement, mode);
        }

        internal static ValueTask<string> GetQuillEmbedded(
            IJSRuntime jsRuntime)
        {
            return jsRuntime.InvokeAsync<string>(strGetQuillEmbedded);
        }

        internal static ValueTask<string> CreateQuillEmbedded(
            IJSRuntime jsRuntime, ElementReference quillElement, string url, string img, string title)
        {
            return jsRuntime.InvokeAsync<string>(strCreateQuillEmbedded, quillElement, url, img, title);
        }

        internal static ValueTask ClearQuillEmbedded(IJSRuntime jSRuntime, ElementReference quillElement)
        {
            return jSRuntime.InvokeVoidAsync(strClearQuillEmbedded, quillElement);
        }

        //internal static ValueTask<string> GetQuillAttachedFiles(IJSRuntime jSRuntime, ElementReference quillElement)
        //{
        //    var ret = jSRuntime.InvokeAsync<string>(strGetQuillAttachedFiles, quillElement);
        //    Console.WriteLine("FROM QUILLINTEROP: ");
        //    Console.WriteLine(ret);
        //    return ret;
        //}
    }
}