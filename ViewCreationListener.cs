using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics;
using System;

namespace GradientSelection
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
            new FormatMapWatcher(textView, formatMapService.GetEditorFormatMap(textView));
        }
    }

    internal sealed class FormatMapWatcher
    {
        bool inUpdate = false;
        IEditorFormatMap formatMap;
        ITextView view;

        static Brush gradientBrush;
        static Pen gradientBorder;

        static FormatMapWatcher()
        {
            Color highlight = SystemColors.HighlightColor;
            Color darkColor = Color.FromArgb(96, highlight.R, highlight.G, highlight.B);
            Color lightColor = Color.FromArgb(48, highlight.R, highlight.G, highlight.B);

            gradientBrush = new LinearGradientBrush(new GradientStopCollection() { new GradientStop(darkColor, 0.0), 
                                                                                   new GradientStop(lightColor, 0.5), 
                                                                                   new GradientStop(darkColor, 1.0) },
                                                    90.0);
            gradientBorder = new Pen(SystemColors.HighlightBrush, 1) { LineJoin = PenLineJoin.Round };

            gradientBrush.Freeze();
            gradientBorder.Freeze();
        }

        public FormatMapWatcher(ITextView view, IEditorFormatMap formatMap)
        {
            this.formatMap = formatMap;
            this.view = view;
            
            this.SetAppropriateBrush();

            formatMap.FormatMappingChanged += (sender, args) => SetAppropriateBrush();

            // Track the rich client option changing, to set and clear the gradient brush
            view.Options.OptionChanged += (sender, args) =>
            {
                if (args.OptionId == DefaultWpfViewOptions.EnableSimpleGraphicsId.Name)
                {
                    SetAppropriateBrush();
                }
            };
        }

        void SetAppropriateBrush()
        {
            if (inUpdate)
                return;

            try
            {
                inUpdate = true;

                if (!view.Options.GetOptionValue(DefaultWpfViewOptions.EnableSimpleGraphicsId))
                {
                    SetGradientBrush();
                }
                else
                {
                    ClearGradientBrush();
                }
            }
            finally
            {
                inUpdate = false;
            }
        }

        void SetGradientBrush()
        {
            // Change the selected text properties to use a gradient brush and an outline pen
            var properties = formatMap.GetProperties("Selected Text");
            properties[EditorFormatDefinition.BackgroundBrushId] = gradientBrush;
            properties["BackgroundPen"] = gradientBorder;
            formatMap.SetProperties("Selected Text", properties);
        }

        void ClearGradientBrush()
        {
            // Clear out the gradient brush and outline pen
            var properties = formatMap.GetProperties("Selected Text");

            if (properties[EditorFormatDefinition.BackgroundBrushId] == gradientBrush)
                properties.Remove(EditorFormatDefinition.BackgroundBrushId);
            if (properties["BackgroundPen"] == gradientBorder)
                properties.Remove("BackgroundPen");

            formatMap.SetProperties("Selected Text", properties);
        }
    }
}