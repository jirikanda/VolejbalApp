import * as React from "react";
import SimpleAppBar from "./SimpleAppBar";

export default class Layout extends React.Component {
    render() {
        return <div className="container">
            <SimpleAppBar />
            {this.props.children}
        </div>;
    }
}