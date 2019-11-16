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
        private IHierarchyBridge _hierarchy;
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

            IsNew = true;
            SaveCommand = new DelegateCommand(Save, CanSave);
        }

        public bool IsNew { get; private set; }
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

        public void Create(IHierarchyBridge hierarchy)
        {
            _hierarchy = hierarchy;
        }
        public void Edit(PipelineFont font)
        {
            _font = font;
            _name = font.Name;
            _family = font.Family;
            _size = font.Size;
            _style = font.Style;

            IsNew = false;
        }

        private void Save()
        {
            if (_font == null)
                _font = _manager.AddFileOnSelectedPath<PipelineFont>(_hierarchy, Name);

            _font.Family = Family;
            _font.Size = Size;
            _font.Spacing = Spacing;
            _font.Style = Style;

            _font.Save();
            _manager.BuildPipeline();

            Close();
        }
        private bool CanSave()
        {
            return Name.HasValue() && Size > 0 && Spacing >= 0;
        }
    }
}