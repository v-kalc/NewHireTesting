// <copyright file="view-feedback-api.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { getBaseUrl } from '../configVariables';

let baseAxiosUrl = getBaseUrl() + '/api';

/**
* Get feedback data.
*/
export const getFeedbackData = async (): Promise<any> => {
    let url = `${baseAxiosUrl}/feedback`;
    return await axios.get(url);
}