﻿using System.ComponentModel.Design;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace SimpleLyricsMaker.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<EditViewModel>();
            SimpleIoc.Default.Register<TimePointViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public EditViewModel Edit => ServiceLocator.Current.GetInstance<EditViewModel>();
        public TimePointViewModel TimePoint => ServiceLocator.Current.GetInstance<TimePointViewModel>();
    }
}