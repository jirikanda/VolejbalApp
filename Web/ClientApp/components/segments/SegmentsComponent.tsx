import * as React from "react";
import { connect } from "react-redux";
import SegmentCard from "./SegmentCard";
import {SegmentVM, ProductType} from '../../apimodels/SegmentsStore';
import { ApplicationState } from "../../store";
import { NavLink, Link, withRouter, RouteComponentProps } from "react-router-dom";
import { Container, Row, Col } from "reactstrap";

require("./../../src/img/hypotecni_uvery.png");
require("./../../src/img/pojisteni_aut.png");
require("./../../src/img/pojisteni_osob.png");
require("./../../src/img/pojisteni_privatniho_majetku.png");
require("./../../src/img/spotrebitelske_uvery.png");
require("./../../src/img/stavebni_sporeni.png");

type SegmentsProps =
    RouteComponentProps<{}>
    & {
        segments: SegmentVM[];
        selectProductType: (productType: ProductType) => void;
    }

const SegmentsComponent = (props: SegmentsProps) => {
    const { segments } = props;

    const onSegmentCardWrapperClick = (event: React.MouseEvent<HTMLElement>, segmentId: number, productTypeId: number) => {
        const isLinkClicked = (event.target instanceof HTMLAnchorElement)
        if (!isLinkClicked) {
            props.history.push(`/segments/${segmentId}/product-types/${productTypeId}`)
        }
    }

    return (
        <div>
            <Row>
                {segments.filter(segment => segment.isVisible).map((segment, index) =>
                    <Col key={segment.id} className="mb-4" md="4">
                        <SegmentCard {...props} segment={segment} onSegmentCardWrapperClick={(event) => onSegmentCardWrapperClick(event, segment.id, segment.productTypes[0].id)} />
                    </Col>
                )}
            </Row>
        </div>
    );
};

export default withRouter(SegmentsComponent);
