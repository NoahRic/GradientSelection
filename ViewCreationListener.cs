using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ItalicComments
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("any")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ViewCreationListener : IWpfTextViewCreationListener
    {
        [Import]
        IEditorFormatMapService formatMapService = null;

        /// <summary>
        /// When a text view is created, update its format map.
        /// </summary>
        public void TextViewCreated(IWpfTextView textView)
        {
            var formatMap = formatMapService.GetEditorFormatMap(textView);

            // Set the gradient brush only if the rich client experience is enabled
            if (!textView.Options.GetOptionValue(DefaultWpfViewOptions.EnableSimpleGraphicsId))
            {
                SetGradientBrush(formatMap);
            }

            // Track the rich client option changing, to set and clear the gradient brush
            textView.Options.OptionChanged += (sender, args) =>
                {
                    if (args.OptionId == DefaultWpfViewOptions.EnableSimpleGraphicsId.Name)
                    {
                        if (!textView.Options.GetOptionValue(DefaultWpfViewOptions.EnableSimpleGraphicsId))
                        {
                            SetGradientBrush(formatMap);
                        }
                        else
                        {
                            ClearGradientBrush(formatMap);
                        }
                    }
                };
        }

        void SetGradientBrush(IEditorFormatMap formatMap)
        {
            Color highlight = SystemColors.HighlightColor;
            Color darkColor = Color.FromArgb(96, highlight.R, highlight.G, highlight.B);
            Color lightColor = Color.FromArgb(48, highlight.R, highlight.G, highlight.B);

            // Change the selected text properties to use a gradient brush and an outline pen
            var properties = formatMap.GetProperties("Selected Text");
            properties[EditorFormatDefinition.BackgroundBrushId] = new LinearGradientBrush(
                new GradientStopCollection() { new GradientStop(darkColor, 0.0), new GradientStop(lightColor, 0.5), new GradientStop(darkColor, 1.0) },
                90.0);
            properties["BackgroundPen"] = new Pen(SystemColors.HighlightBrush, 1) { LineJoin = PenLineJoin.Round };
            formatMap.SetProperties("Selected Text", properties);
        }

        void ClearGradientBrush(IEditorFormatMap formatMap)
        {
            // Clear out the gradient brush and outline pen
            var properties = formatMap.GetProperties("Selected Text");
            properties.Remove(EditorFormatDefinition.BackgroundBrushId);
            properties.Remove("BackgroundPen");
            formatMap.SetProperties("Selected Text", properties);
        }
    }
}