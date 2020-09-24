// <copyright file="router.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Suspense } from "react";
import { BrowserRouter, Route, Switch } from "react-router-dom";
import SignInPage from "../components/signin/signin";
import SignInSimpleStart from "../components/signin/signin-start";
import SignInSimpleEnd from "../components/signin/signin-end";
import Redirect from "../components/redirect";
import ErrorPage from "../components/error-page";
import ViewFeedbackPage from "../components/view-feedback/view-feedback-page";
import DownloadFeedbackPage from "../components/view-feedback/download-feedback-page";

export const AppRoute: React.FunctionComponent<{}> = () => {

    return (
        <Suspense fallback={<div className="container-div"><div className="container-subdiv"></div></div>}>
            <BrowserRouter>
                <Switch>
                    <Route exact path="/signin" component={SignInPage} />
                    <Route exact path="/signin-simple-start" component={SignInSimpleStart} />
                    <Route exact path="/signin-simple-end" component={SignInSimpleEnd} />
                    <Route exact path="/error" component={ErrorPage} />
                    <Route exact path="/view-feedback" component={ViewFeedbackPage} />
                    <Route exact path="/download-feedback" component={DownloadFeedbackPage} />
                    <Route component={Redirect} />
                </Switch>
            </BrowserRouter>
        </Suspense>
    );
}
