import * as React from "react";
import { Card, CardImg, CardText, CardBody, CardLink, CardTitle, CardSubtitle } from "reactstrap";
import { SegmentVM, ProductType } from '../../apimodels/SegmentsStore';
import { NavLink, Link, withRouter, RouteComponentProps } from "react-router-dom";
import { push, replace } from 'react-router-redux'
import * as classNames from "classNames";

type SegmentCardProps = RouteComponentProps<{}> & {
    segment: SegmentVM;
    onSegmentCardWrapperClick: (event: React.MouseEvent<HTMLElement>) => void;
    selectProductType: (productType: ProductType) => void;
 }

class SegmentCard extends React.Component<SegmentCardProps> {
    segmentCardItemClick = (productType: ProductType) => (event: React.MouseEvent<HTMLElement>) => {
        this.props.selectProductType(productType);
        event.stopPropagation();
    };

    render() {
        const {segment, onSegmentCardWrapperClick} = this.props;

        const classes = classNames("card-segment", { "disabled": !segment.hasPermissionToRead });

        return <div onClick={(event) => segment.hasPermissionToRead && onSegmentCardWrapperClick(event)}>
            <Card className={classes}>
                <CardBody>
                    <div className="text-center my-4"><img src={`/dist/src/img/${segment.image}`} /></div>
                    <CardTitle className="text-center mb-4">{segment.name}</CardTitle>
                    <ul>
                        {segment.productTypes.map((type, index) =>
                            <li onClick={this.segmentCardItemClick(type)} key={type.id}>
                                <NavLink to={`/segments/${segment.id}/product-types/${type.id}`}>{type.name}</NavLink>
                            </li>
                        )}
                    </ul>
                </CardBody>
            </Card>
        </div>
    }
};

export default SegmentCard;