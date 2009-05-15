using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SimpleOrgChart___Final.App;
using SimpleOrgChart___Final.App.NewEmployeeProcess;
using SimpleOrgChart___Final.AppController;
using SimpleOrgChart___Final.Model;
using SpecUnit;

namespace SimpleOrgChart___Final.UnitTests
{
	public class AddNewEmployeeSpecs
	{

		public class KickoffAddNewEmployeeContext : ContextSpecification
		{

			private IEmployeeRepository employeeRepo;
			private IOrgChartView view;
			private IList<Employee> employeeList;
			private Employee bob;
			protected IApplicationController appController;

			protected override void SharedContext()
			{
				view = MockRepository.GenerateMock<IOrgChartView>();
				bob = new Employee("Bob", "Jones", "bob.jones@example.com");
				employeeList = new List<Employee> { bob };

				appController = MockRepository.GenerateMock<IApplicationController>();

				employeeRepo = MockRepository.GenerateMock<IEmployeeRepository>();
				employeeRepo.Stub(r => r.GetEmployeeOrgChart()).Return(employeeList);

			}

			protected OrgChartPresenter GetPresenter()
			{
				OrgChartPresenter presenter = new OrgChartPresenter(view, appController, employeeRepo);
				return presenter;
			}

		}

		public class AddNewEmployeeCommandContext : ContextSpecification
		{
			protected IAddNewEmployeeService addNewEmployeeService;

			protected AddNewEmployeeCommand GetAddNewEmployeeCommand()
			{
				addNewEmployeeService = MockRepository.GenerateMock<IAddNewEmployeeService>();

				AddNewEmployeeCommand addNewEmployeeCommand = new AddNewEmployeeCommand(addNewEmployeeService);
				return addNewEmployeeCommand;
			}

		}

		public class AddNewEmployeeSpecsContext : ContextSpecification
		{
			protected IGetNewEmployeeInfo getNewEmployeeInfo;
			protected IGetEmployeeManager getEmployeeManager;
			protected IEmployeeRepository employeeRepository;
			protected Employee newEmployee;
			protected IApplicationController appController;
			protected EmployeeInfo employeeInfo;

			protected override void SharedContext()
			{
				employeeInfo = new EmployeeInfo { FirstName = "Jim", LastName = "Jones", Email = "jim.jones@example.com" };
				getNewEmployeeInfo = MockRepository.GenerateMock<IGetNewEmployeeInfo>();

				getEmployeeManager = MockRepository.GenerateMock<IGetEmployeeManager>();
				getEmployeeManager.Stub(g => g.Get()).Return(new Employee("Bob", "Jones", "bob.jones@example.com"));

				employeeRepository = MockRepository.GenerateMock<IEmployeeRepository>();
				employeeRepository.Stub(r => r.Save(null)).IgnoreArguments().Do(new Action<Employee>(e => newEmployee = e));

				appController = MockRepository.GenerateMock<IApplicationController>();
			}

			protected IAddNewEmployeeService GetAddNewEmployeeService()
			{
				AddNewEmployeeService service = new AddNewEmployeeService(getNewEmployeeInfo, getEmployeeManager, employeeRepository, appController);
				return service;
			}
		}

		[TestFixture]
		[Concern("Add New Employee")]
		public class When_requesting_to_add_a_new_employee : KickoffAddNewEmployeeContext
		{

			protected override void Context()
			{
				OrgChartPresenter presenter = GetPresenter();
				presenter.Run();

				presenter.AddNewEmployeeRequested();
			}

			[Test]
			[Observation]
			public void Should_kick_off_the_add_new_employee_process()
			{
				appController.AssertWasCalled(c => c.Execute<AddNewEmployeeData>(null), mo => mo
					.IgnoreArguments()
					.Constraints(Is.TypeOf<AddNewEmployeeData>())
				);
			}

		}

		[TestFixture]
		[Concern("Add New Employee")]
		public class When_kicking_off_the_add_new_employee_process : AddNewEmployeeCommandContext
		{

			protected override void Context()
			{
				AddNewEmployeeCommand addNewEmployeeCommand = GetAddNewEmployeeCommand();
				addNewEmployeeCommand.Execute(new AddNewEmployeeData());
			}

			[Test]
			[Observation]
			public void Should_run_the_add_new_employee_service()
			{
				addNewEmployeeService.AssertWasCalled(s => s.Run());
			}

		}

		[TestFixture]
		[Concern("Add New Employee")]
		public class When_adding_a_new_employee : AddNewEmployeeSpecsContext
		{

			protected override void Context()
			{
				Result<EmployeeInfo> result = new Result<EmployeeInfo>(ServiceResult.Ok, employeeInfo);
				getNewEmployeeInfo.Stub(g => g.Get()).Return(result);
				IAddNewEmployeeService service = GetAddNewEmployeeService();
				service.Run();
			}

			[Test]
			[Observation]
			public void Should_request_the_employee_information()
			{
				getNewEmployeeInfo.AssertWasCalled(x => x.Get());
			}

			[Test]
			[Observation]
			public void Should_request_the_employees_manager()
			{
				getEmployeeManager.AssertWasCalled(x => x.Get());
			}

			[Test]
			[Observation]
			public void Should_save_the_new_employee()
			{
				employeeRepository.AssertWasCalled(r => r.Save(null), mo => mo
					.IgnoreArguments()
					.Constraints(Is.TypeOf<Employee>())
				);

				newEmployee.FirstName.ShouldEqual("Jim");
				newEmployee.LastName.ShouldEqual("Jones");
				newEmployee.Email.ShouldEqual("jim.jones@example.com");
			}

			[Test]
			[Observation]
			public void Should_notify_of_new_employee_added()
			{
				appController.AssertWasCalled(a => a.Raise<EmployeeAddedEvent>(null), mo => mo
					.IgnoreArguments()
					.Constraints(Is.TypeOf<EmployeeAddedEvent>())
				);
			}

		}

		[TestFixture]
		[Concern("Add New Employee")]
		public class When_cancelling_the_addition_of_a_new_employee : AddNewEmployeeSpecsContext
		{

			protected override void Context()
			{
				Result<EmployeeInfo> result = new Result<EmployeeInfo>(ServiceResult.Cancel);
				getNewEmployeeInfo.Stub(g => g.Get()).Return(result);
				IAddNewEmployeeService service = GetAddNewEmployeeService();
				service.Run();
			}

			[Test]
			[Observation]
			public void Should_not_save_the_employee()
			{
				employeeRepository.AssertWasNotCalled(r => r.Save(null), mo => mo.IgnoreArguments());
			}

			[Test]
			[Observation]
			public void Should_not_notify_of_new_employee_added()
			{
				appController.AssertWasNotCalled(a => a.Raise<EmployeeAddedEvent>(null), mo => mo.IgnoreArguments());
			}

		}

	}
}