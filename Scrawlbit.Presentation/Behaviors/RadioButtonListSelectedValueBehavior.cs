﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using Scrawlbit.Presentation.Commands;
using Scrawlbit.Presentation.Dependency;
using Scrawlbit.Presentation.Helpers;

namespace Scrawlbit.Presentation.Behaviors
{
    public class RadioButtonListSelectedValueBehavior : Behavior<Panel>
    {
        private static readonly DependencyProperty<RadioButtonListSelectedValueBehavior, string> GroupNameProperty;
        private static readonly DependencyProperty<RadioButtonListSelectedValueBehavior, object> SelectedValueProperty;
        private List<RadioButton> _radioButtons;

        static RadioButtonListSelectedValueBehavior()
        {
            var dependency = new DependencyRegistry<RadioButtonListSelectedValueBehavior>();

            GroupNameProperty = dependency.Property(b => b.GroupName);
            SelectedValueProperty = dependency.Property(b => b.SelectedValue, b => b.OnSelectedValueChanged());
        }

        public string GroupName
        {
            get { return GroupNameProperty.Get(this); }
            set { GroupNameProperty.Set(this, value); }
        }
        public object SelectedValue
        {
            get { return SelectedValueProperty.Get(this); }
            set { SelectedValueProperty.Set(this, value); }
        }

        private void OnSelectedValueChanged()
        {
            if (_radioButtons != null)
                _radioButtons.ForEach(r => r.IsChecked = Equals(r.Tag, SelectedValue));
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnPanelLoaded;
        }
        private void OnPanelLoaded(object sender, RoutedEventArgs e)
        {
            var radioButtons = AssociatedObject.FindChildren<RadioButton>();
            radioButtons = radioButtons.Where(r => r.GroupName == GroupName).ToList();

            if (!radioButtons.Any())
                return;

            _radioButtons = (List<RadioButton>)radioButtons;
            _radioButtons.ForEach(r => r.Command = new DelegateCommand(() => SelectedValue = r.Tag));
            OnSelectedValueChanged();

            AssociatedObject.Loaded -= OnPanelLoaded;
        }
    }
}