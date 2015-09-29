# USDA-ARS Umbraco Project Management with GitHub

We've tried a lot of project management systems over the years. In some way, they have always seemed lacking, confusing or just a pain in the rear end. If they had good tools for project managers, they were confusing to developers. If they were useful for developers, designers complained about the eye-sores. No one system ever seemed to satisfy the team.

We recently started using GitHub for project management after the developers started raving about how much they loved it for managing code. To our surprise, GitHub has proven a solid option for project management. Our designers have even started using it for some of their projects, which I think says something about GitHub's aesthetics. With a little bit of something for each role, GitHub is starting to come out on top as the tool of choice for hosting code, managing projects, and facilitating project communication.

## Project Introductions
GitHub is pretty developer-centric. As such, the first thing a developer sees when they open a project, is a view of the code repository. Below that, GitHub automatically renders the README file found in the root of the code base. It's a very typical practice for software projects, especially open source software projects, to have this file in place. The README can be in various formats, but a favorite of mine is Markdown. Simply giving the README an extension of .md tells GitHub to render your README.md using the Markdown syntax. Even better, GitHub has it's own flavor of markdown. Since the developers of your project see the README first, this is a great place for information that will get them up and running with the project as quickly as possible. Be concise. If you need to write more than a few sentences, chances are, you should be linking off to more in-depth documentation in your project's wiki. Here's a quick guideline of some of the things that you might want to include in your README.

## A quick project overview.

Provide a short description of the project's goals and a bit of background. Any links that you frequently access are also good to include up at the top as well, for easy access. Everyone loves easy access.

## Information about the directory structure.

Typically we have more than just Drupal in our repository root, so it's helpful to have a brief description of what is in there. We typically have a drush folder for aliases and commands, as well as a patches directory with its own README.

## How to get started developing.

Tell the developers what the best way to jump into the project might be. Things like, "clone this repository, create a feature branch, run the installer, download a copy of the database, etc.. Whomever reviews the pull request should also do things like remove the remote branch from the repository once it is merged."

## Code promotion workflow.

The workflow below outlines the development process.
![Code promotion workflow](/images/code promotion workflow.jpg)

**New Issues** This Pipeline is the landing point for new Issues. We have a weekly triage meeting to review and prioritize all Issues in this pipeline. Anyone from the team can create an Issue at any time and know that, through this process, it will be visible to everyone. The triage meeting should always end with an empty 'New Issues' pipeline! 

**Icebox** The Icebox represents items that are a low priority in the product backlog. We don't want to delete these and create a cycle of raising duplicate issues, so we keep them in our icebox with just enough information attached that we can pick it up some time in the future -- if and when we choose to do so. 

Icebox Issues should not take up a team member’s time or mental bandwidth; we find that putting ideas into the Icebox Pipeline gets them out of our heads and helps us focus on immediate priorities.

**Backlog** This Pipeline is a prioritized backlog of items ready for development. The Backlog is used heavily during sprint planning meetings: the higher an issue is on this list, the higher the priority. Higher-priority items will typically have more in-depth information attached, and we keep all our use cases and requirements in the Issue comments. 

**In Progress** This one is self-explanatory! Each Issue in this pipeline should have an assigned owner who is responsible for its completion. If a team member decides to take on a task, she or he simply self-assigns the Issue and moves it to the In Progress column, instantly communicating to the rest of the team that the task is underway. 

**Review/QA** We use the Review/QA column for Issues that are open to the team for review and testing. Usually this means the code is deployed into our Staging environment and in-use by the 40+ member Axiom Zen team spread across the world. 

**Done** Issues in this pipeline need no further work and are ready to be closed. Having a good ‘Definition of Done’ agreed upon before work starts on an Issue is very helpful here! If there were any objectives or key metrics associated with the Issue, they can be appended prior to closing. 

## Environments.

Outline information for your dev, staging and live environments, if you have them. Also, outline the process for getting things to the various places. How do I make sure my code is on staging? What is the best way to grab a database dump? We like to setup drush aliases for each environment ahead of time as a means of outlining this information and giving developers a good starting point. This document contains some example commands for doing some typical operations. Here's an example.

## Links to where to find more information.

Typically this is our wiki, where we keep more detailed documentation and notes on things; project details like the original proposal's SOW, credentials to environments, Scrum Notes, Pre-launch checklists, etc.

We've attempted to create a drupal-boilerplate, of sorts, for our Drupal projects which we're continuously re-using for new projects and modifying when we find things that work better. Take a look, and if you find it useful, please feel free to use it! If you find anything missing, or have ideas on improving it, please fork it and send us a pull request!
