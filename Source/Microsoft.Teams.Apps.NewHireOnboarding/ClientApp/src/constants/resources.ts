﻿// <copyright file="resources.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

export interface IPostType {
	name: string;
	id: string;
	color: string;
}

export default class Resources {

	// Themes
	public static readonly body: string = "body";
	public static readonly theme: string = "theme";
	public static readonly default: string = "default";
	public static readonly light: string = "light";
	public static readonly dark: string = "dark";
	public static readonly contrast: string = "contrast";

    // Screen size
    public static readonly screenWidthLarge: number = 1200;
    public static readonly screenWidthSmall: number = 1000;

}