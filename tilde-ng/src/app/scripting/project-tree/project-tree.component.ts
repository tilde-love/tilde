import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {Subscription, BehaviorSubject, from, of as observableOf} from 'rxjs';
import { NestedTreeControl } from '@angular/cdk/tree';
import {Project, ProjectDataService} from '../project-data.service';

export class FileNode {
  public children: { [ name: string]: FileNode } = {};
  public filename: string;
  public uri: any;
  public isOpen: boolean;
  public isFile: boolean;
  public icon: string;
  public color: string;
}

@Component({
  selector: 'app-project-tree',
  templateUrl: './project-tree.component.html',
  styleUrls: ['./project-tree.component.scss'],
})
export class ProjectTreeComponent implements OnInit, OnDestroy {

  @Input('project') project: BehaviorSubject<Project>;
  @Input('active') activePath: string;

  treeControl: NestedTreeControl<FileNode> =
    new NestedTreeControl<FileNode>(
      node => observableOf(Object.values(node.children))
      );

      //   Object
      //   .keys(node.children)
      //   .map((key) => {
      //   return node.children[key];
      // })));

  dataSource: FileNode[] = [];
  treeState: { [uri: string]: boolean } = {};
  private _projectSubscription: Subscription;

  constructor(private projectDataService: ProjectDataService) {

  }

  toggle(node: FileNode) {
    node.isOpen = !node.isOpen;
    this.treeState[node.uri] = node.isOpen;
    ProjectDataService.setProjectTreeState(this.project.getValue(), this.treeState);
  }

  ngOnInit(): void {

    this._projectSubscription = this.project.subscribe((project) => {
      this.treeState = ProjectDataService.getProjectTreeState(project);

      const nodes: FileNode[] = [];

      let uri = `/projects/${project.uri}`;
      for (const fileNode of this.buildFileTree(uri, project.files)) {
        nodes.push(fileNode);
      }

      uri = `/projects/${project.uri}/readme`;
      const readme: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'readme', isFile: true, uri: uri, icon: 'insert_drive_file',
        color: '',
        children: {}
      };

      uri = `/projects/${project.uri}/log`;
      const log: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'log', isFile: true, uri: uri, icon: 'receipt',
        color: '',
        children: {}
      };

      uri = `/projects/${project.uri}/build`;
      const build: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'build', isFile: true, uri: uri, icon: 'feedback',
        color: '',
        children: {}
      };

      uri = `/projects/${project.uri}/settings`;
      const settings: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'settings', isFile: true, uri: uri, icon: 'settings',
        color: '',
        children: {}
      };

      nodes.push(readme);
      nodes.push(log);
      nodes.push(build);
      nodes.push(settings);

      this.dataSource = nodes;
    });
  }

  private buildFileTree(baseUri: string, values: string[]): FileNode[] {

    values.sort();

    const folders: string[] = Array.from(values, (path: string) => path.substring(0, path.lastIndexOf('/')))
      .filter((v, i, a) => v !== '' && a.indexOf(v) === i);

    const folderPathArrays: string[][] = Array.from(folders, (path: string) => path.split('/'));
    const pathArrays: string[][] = Array.from(values, (path: string) => path.split('/'));

    const root: FileNode = {
      isOpen: false,
      filename: 'ROOT',
      isFile: false,
      uri: baseUri,
      icon: 'ROOT',
      color: '',
      children: {}
    };

    for (const path of folderPathArrays) {
      let parent: FileNode = root;
      let uri: string = baseUri;

      for (const segment of path.slice(0, path.length)) {
        uri += `/${segment}`;

        let match: FileNode = parent.children[segment];

        if (match === undefined) {
          match = {
            isOpen: this.treeState[uri],
            filename: segment,
            isFile: false,
            uri: uri,
            icon: '',
            color: '',
            children: {}
          };

          parent.children[segment] = match;
        }

        parent = match;
      }
    }

    for (const path of pathArrays) {
      let parent: FileNode = root;

      for (const segment of path.slice(0, path.length - 1)) {
        parent = parent.children[segment];
      }

      const filename: string = path[path.length - 1];
      const uri = `${parent.uri}/${filename}`;

      parent.children[filename] = {
        isOpen: this.treeState[uri],
        filename: filename,
        isFile: true,
        uri: uri,
        icon: 'insert_drive_file',
        color: 'accent',
        children: {}
      };
    }

    return Object.values(root.children);
  }

  ngOnDestroy(): void {
    this._projectSubscription.unsubscribe();
  }
}
