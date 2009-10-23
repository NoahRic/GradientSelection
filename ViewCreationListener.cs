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

        public void TextViewCreated(IWpfTextView textView)
        {
            var formatMap = formatMapService.GetEditorFormatMap(textView);

            if (!textView.Options.GetOptionValue(DefaultWpfViewOptions.EnableSimpleGraphicsId))
            {
                SetGradientBrush(formatMap);
            }

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
            var properties = formatMap.GetProperties("Selected Text");
            properties[EditorFormatDefinition.BackgroundBrushId] = new LinearGradientBrush(
                new GradientStopCollection() { new GradientStop(Colors.LightSkyBlue, 0.0), new GradientStop(Colors.AliceBlue, 0.5), new GradientStop(Colors.LightSkyBlue, 1.0) },
                90.0);
            properties["BackgroundPen"] = new Pen(Brushes.Blue, 0.5);
            formatMap.SetProperties("Selected Text", properties);
        }

        void ClearGradientBrush(IEditorFormatMap formatMap)
        {
            var properties = formatMap.GetProperties("Selected Text");
            properties.Remove(EditorFormatDefinition.BackgroundBrushId);
            properties.Remove("BackgroundPen");
            formatMap.SetProperties("Selected Text", properties);
        }
    }
}