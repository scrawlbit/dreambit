using DreamBit.Extension.Components;
using DreamBit.Extension.Management;
using DreamBit.Pipeline.Files;
using Scrawlbit.Helpers;
using Scrawlbit.Presentation.Commands;
using System.Windows.Input;
using DreamBit.Resources;

namespace DreamBit.Extension.ViewModels.Dialogs
{
    public class EditFontDialogViewModel : BaseDialogViewModel
    {
        private readonly IProjectManager _manager;
        private PipelineFont _font;
        private string _name;
        private FontFamily _family;
        private int _size;
        private int _spacing;
        private FontStyle _style;

        public EditFontDialogViewModel(IProjectManager manager)
        {
            _manager = manager;
            _name = PipelineResources.Segoe;
            _family = FontFamily.SegoeUI;
            _size = 12;
            _style = FontStyle.Regular;

            SaveCommand = new DelegateCommand(Save, CanSave);
        }

        public IHierarchyBridge Hierarchy { get; set; }
        public PipelineFont Font
        {
            get => _font;
            set => Set(ref _font, value);
        }
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }
        public FontFamily Family
        {
            get => _family;
            set => Set(ref _family, value);
        }
        public int Size
        {
            get => _size;
            set => Set(ref _size, value);
        }
        public int Spacing
        {
            get => _spacing;
            set => Set(ref _spacing, value);
        }
        public FontStyle Style
        {
            get => _style;
            set => Set(ref _style, value);
        }
        public ICommand SaveCommand { get; }

        private void Save()
        {
            if (Font == null)
                Font = _manager.AddFileOnSelectedPath<PipelineFont>(Hierarchy, Name);

            Font.Family = Family;
            Font.Size = Size;
            Font.Spacing = Spacing;
            Font.Style = Style;

            Font.Save();
            _manager.BuildPipeline();

            Close();
        }
        private bool CanSave()
        {
            return Name.HasValue() && Size > 0 && Spacing >= 0;
        }
    }
}