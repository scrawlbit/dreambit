using Microsoft.VisualStudio.Shell;
using Scrawlbit.Presentation.Commands;
using System;
using System.ComponentModel.Design;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;

namespace DreamBit.Extension.Components
{
    internal interface IToolCommand : ICommand
    {
        Task RegisterAsync(IPackageBridge package);
    }

    internal abstract class ToolCommand : BaseCommand, IToolCommand
    {
        private OleMenuCommand _command;

        protected abstract int Id { get; }

        protected virtual bool CanShow() => true;
        public virtual bool CanExecute() => true;
        public abstract void Execute();

        async Task IToolCommand.RegisterAsync(IPackageBridge package)
        {
            await package.SwitchToMainThreadAsync();

            var commandService = await package.GetServiceAsync<IMenuCommandService>();
            var commandId = new CommandID(new Guid(DreamBitPackage.Guids.CommandSet), Id);

            _command = new OleMenuCommand(Clicked, commandId);
            _command.BeforeQueryStatus += (s, e) => QueryStatus();

            commandService.AddCommand(_command);
        }

        protected void QueryStatus()
        {
            _command.Visible = CanShow();
            _command.Enabled = CanExecute();
        }

        private void Clicked(object sender, EventArgs e)
        {
            Execute();
        }
    }
}