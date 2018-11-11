import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {Subscription, BehaviorSubject, from, of as observableOf} from 'rxjs';
import { NestedTreeControl } from '@angular/cdk/tree';
import {ProjectDataService} from '../project-data.service';
import {Project} from '../project-types';

export enum FileType {
  File = 0,
  Folder,
  Partial,
}

export class FileNode {
  public children: { [ name: string]: FileNode } = {};
  public filename: string;
  public uri: any;
  public isOpen: boolean;
  public fileType: FileType;
  public icon: string;
  public color: string;
}

@Component({
  selector: 'app-project-tree',
  templateUrl: './project-tree.component.html',
  styleUrls: ['./project-tree.component.scss'],
})
export class ProjectTreeComponent implements OnInit, OnDestroy {

  fileType = FileType;

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
        filename: 'readme', fileType: FileType.File, uri: uri, icon: 'insert_drive_file',
        color: '',
        children: {}
      };

      uri = `/projects/${project.uri}/log`;
      const log: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'log', fileType: FileType.File, uri: uri, icon: 'receipt',
        color: '',
        children: {}
      };

      uri = `/projects/${project.uri}/build`;
      const build: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'build', fileType: FileType.File, uri: uri, icon: 'feedback',
        color: '',
        children: {}
      };

      uri = `/projects/${project.uri}/settings`;
      const settings: FileNode = {
        isOpen: this.treeState[uri],
        filename: 'settings', fileType: FileType.File, uri: uri, icon: 'settings',
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
      fileType: FileType.Folder,
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
            fileType: FileType.Folder,
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

      if (filename.endsWith('.panel.json')) {
        const panelFilename = filename.substring(0, filename.length - '.json'.length);
        const panelUri = `${parent.uri}/${panelFilename}`;

        const partial = {
          isOpen: this.treeState[panelUri],
          filename: panelFilename,
          fileType: FileType.Partial,
          uri: panelUri,
          icon: 'tune',
          color: '',
          children: {}
        };
        partial.children[filename] = {
          isOpen: this.treeState[uri],
          filename: filename,
          fileType: FileType.File,
          uri: uri,
          icon: 'insert_drive_file',
          color: 'accent',
          children: {}
        };

        parent.children[panelFilename] = partial;

      } else {
        parent.children[filename] = {
          isOpen: this.treeState[uri],
          filename: filename,
          fileType: FileType.File,
          uri: uri,
          icon: 'insert_drive_file',
          color: 'accent',
          children: {}
        };
      }
    }

    return Object.values(root.children);
  }

  ngOnDestroy(): void {
    this._projectSubscription.unsubscribe();
  }
}
