import * as React from "react";
import * as ReactDOM from "react-dom";

interface ScrollHandlerProps {
    onScroll: (event: UIEvent, element: Element) => void;
}

export default class ScrollHandler extends React.Component<ScrollHandlerProps> {
    componentDidMount() {
        window.addEventListener('scroll', this.handleScroll);
    }

    componentWillUnmount() {
        window.removeEventListener('scroll', this.handleScroll);
    }

    handleScroll = (event: UIEvent) => {
        const child = ReactDOM.findDOMNode(this);
        this.props.onScroll(event, child)
    }

    render() {
        return this.props.children;
    }
}