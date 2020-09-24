// <copyright file="view-feedback-page.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Dialog, Loader } from "@fluentui/react-northstar";
import { getFeedbackData } from "../../api/view-feedback-api";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";
import Spreadsheet from "react-spreadsheet";
import DownloadFeedbackPage from "../view-feedback/download-feedback-page";

import 'bootstrap/dist/css/bootstrap.min.css';
import "../../styles/feedback.css";

let feedbackExcelData = [
    [{ value: "" }, { value: "" }, { value: "" }],];

export interface IFeedbackDetails {
    submittedOn: string,
    feedback: string,
    newHireName: string,
}

interface IFeedbackState {
    isLoading: boolean;
    screenWidth: number;
    feedbackDetails: Array<IFeedbackDetails>,
    DownloadDialogOpen: boolean;
}

class FeedbackPage extends React.Component<WithTranslation, IFeedbackState> {
    localize: TFunction;

    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
        window.addEventListener("resize", this.update);

        this.state = {
            isLoading: true,
            screenWidth: 0,
            feedbackDetails: [],
            DownloadDialogOpen: false,
        }
    }

    /**
    * Used to initialize Microsoft Teams sdk.
    */
    componentDidMount() {
        this.setState({ isLoading: true });
        this.getFeedbackData();
        this.update();
    }

    /**
    * get screen width real time.
    */
    update = () => {
        this.setState({
            screenWidth: window.innerWidth
        });
    };

    /**
    * Fetch share feedback data.
    */
    getFeedbackData = async () => {
        let response = await getFeedbackData();
        if (response.status === 200 && response.data) {
            this.setState(
                {
                    feedbackDetails: response.data
                });
        }

        this.setState({
            isLoading: false
        });
    }

    /**
    *Changes dialog open state to show and hide dialog.
    *@param isOpen Boolean indication whether to show dialog
    */
    changeDialogOpenState = (isOpen: boolean) => {
        this.setState({ DownloadDialogOpen: isOpen })
    }

    /**
    *Changes dialog open state to show and hide dialog.
    *@param isOpen Boolean indication whether to show dialog
    */
    closeDialog = (isOpen: boolean) => {
        this.setState({ DownloadDialogOpen: isOpen })
    }

    renderFeedbacks() {
        if (this.state.feedbackDetails) {

            feedbackExcelData = [
                [{ value: this.localize("columnHeaderMonthText") }, { value: this.localize("columnHeaderNewHireNameText") }, { value: this.localize("columnHeaderFeedbackText") }],
            ];

            this.state.feedbackDetails.forEach(function (feedback) {
                feedbackExcelData.push([{ value: feedback.submittedOn }, { value: feedback.newHireName }, { value: feedback.feedback }]);
            });
        }

        if (this.state.isLoading) {
            return (
                <div className="container-div">
                    <div className="container-subdiv">
                        <div className="loader">
                            <Loader />
                        </div>
                    </div>
                </div>
            );
        }
        else {
            return (
                <div>
                    <div>
                        <Dialog
                            className="dialog-container-close-project"
                            content={<DownloadFeedbackPage closeDialog={this.closeDialog} />}
                            open={this.state.DownloadDialogOpen}
                            onOpen={() => this.setState({ DownloadDialogOpen: false })}
                            trigger={<button onClick={() => this.changeDialogOpenState(true)}> Download feedback </button>}
                        />

                    </div>
                    <div>
                        <Spreadsheet data={feedbackExcelData} />
                    </div>
                </div>
            );
        }
    }

    /**
   * Renders the component.
   */
    public render(): JSX.Element {
        return (
            <div className="container-div">
                <div className="container-subdiv">
                    <div>
                        {this.renderFeedbacks()}
                    </div>
                </div>
            </div>
        );
    }
}

export default withTranslation()(FeedbackPage)