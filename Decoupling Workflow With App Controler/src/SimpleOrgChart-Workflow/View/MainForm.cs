﻿using System.Collections.Generic;
using System.Windows.Forms;
using SimpleOrgChart_Workflow.App;
using SimpleOrgChart_Workflow.App.NewEmployeeProcess;
using SimpleOrgChart_Workflow.App.NewEmployeeProcess.SelectEmployeeManager;
using SimpleOrgChart_Workflow.App.NewEmployeeProcess.SupplyEmployeeInfo;
using SimpleOrgChart_Workflow.Model;

namespace SimpleOrgChart_Workflow.View
{
	public partial class MainForm : Form, IOrgChartView
	{
		private IEmployeeRepository EmployeeRepository { get; set; }

		public OrgChartPresenter Presenter { get; set; }

		public MainForm(IEmployeeRepository repository)
		{
			EmployeeRepository = repository;
			InitializeComponent();
		}

		public void DisplayEmployeeHierarchy(IList<Employee> employees)
		{
			OrgChart.Nodes.Clear();

			foreach(Employee employee in employees)
			{
				TreeNode node = GetNode(employee);
				IList<TreeNode> children = GetChildren(employee);
				foreach (TreeNode childNode in children)
					node.Nodes.Add(childNode);
				OrgChart.Nodes.Add(node);
			}
			OrgChart.ExpandAll();
		}

		private TreeNode GetNode(Employee employee)
		{
			TreeNode node = new TreeNode(employee.DisplayName);
			node.Tag = employee;
			return node;
		}

		private IList<TreeNode> GetChildren(Employee employee)
		{
			IList<TreeNode> nodes = new List<TreeNode>();
			foreach(Employee child in employee.Employees)
			{
				TreeNode node = GetNode(child);
				IList<TreeNode> children = GetChildren(child);
				foreach (TreeNode childNode in children)
					node.Nodes.Add(childNode);
				nodes.Add(node);
			}
			return nodes;
		}

		private void OrgChart_AfterSelect(object sender, TreeViewEventArgs e)
		{
			Employee employee = e.Node.Tag as Employee;
			if (employee != null)
				Presenter.EmployeeSelected(employee);
		}

		private void AddNewEmployee_Click(object sender, System.EventArgs e)
		{
			NewEmployeeInfoForm newEmployeeInfoForm = new NewEmployeeInfoForm();
			NewEmployeeInfoPresenter newEmployeeInfoPresenter = new NewEmployeeInfoPresenter(newEmployeeInfoForm);

			SelectEmployeeManagerForm selectEmployeeManagerForm = new SelectEmployeeManagerForm();
			SelectEmployeeManagerPresenter selectEmployeeManagerPresenter = new SelectEmployeeManagerPresenter(selectEmployeeManagerForm, EmployeeRepository);
			
			IAddNewEmployeeService addNewEmployeeService = new AddNewEmployeeService(newEmployeeInfoPresenter, selectEmployeeManagerPresenter);
			
			Presenter.AddNewEmployeeRequested(addNewEmployeeService);
		}

	}
}