
//TAOBAO_SEARCH_ORDER
(function() {
    var xhr = new XMLHttpRequest();
    var url = "https://trade.taobao.com/trade/itemlist/asyncSold.htm?event_submit_do_query=1&_input_charset=utf8";

    xhr.open("POST", url, false);

    xhr.setRequestHeader("accept", "application/json, text/javascript, */*; q=0.01");
    xhr.setRequestHeader("accept-encoding", "gzip, deflate, br");
    xhr.setRequestHeader("accept-language", "zh-CN,zh;q=0.8");
    xhr.setRequestHeader("content-type", "application/x-www-form-urlencoded; charset=UTF-8");

    //var xhrdata = "action=itemlist/SoldQueryAction&auctionType=0&orderStatus=PAID&tabCode=waitSend&prePageNo=###prePageNo&pageNum=###pageNum&pageSize=15"
    //var xhrdata = "action=itemlist/SoldQueryAction&auctionType=0&orderStatus=SEND&tabCode=haveSendGoods&prePageNo=###prePageNo&pageNum=###pageNum&pageSize=15"
    var xhrdata ="action=itemlist/SoldQueryAction&auctionType=0&orderStatus=SUCCESS&tabCode=success&prePageNo=###prePageNo&pageNum=###pageNum&pageSize=15"
    xhr.send(xhrdata)
    if (xhr.status == 200)
        return xhr.responseText;
    return "ERROR:" + xhr.status;
})();
//TAOBAO_SEARCH_ORDER

//TAOBAO_GET_ORDER
(function() {
    var data = "false";
    var xhr = new XMLHttpRequest();
    var url = "https://trade.taobao.com/detail/orderDetail.htm?bizOrderId=###bizOrderId";

    xhr.open("GET", url, false);

    xhr.setRequestHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
    xhr.setRequestHeader("accept-encoding", "gzip, deflate, br");
    xhr.setRequestHeader("accept-language", "zh-CN,zh;q=0.8");
    xhr.send()
    if (xhr.status == 200)
        return xhr.responseText;
    return "ERROR:" + xhr.status;
})();
//TAOBAO_GET_ORDER

//TAOBAO_MARK_DELIVERY
(function () {
    var data = "false";
    var xhr = new XMLHttpRequest();
    var url = "https://wuliu.taobao.com/user/consign.htm?trade_id=###trade_id";
    xhr.open("GET", url, false);
    xhr.setRequestHeader("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
    xhr.setRequestHeader("accept-encoding", "gzip, deflate, br");
    xhr.setRequestHeader("accept-language", "zh-CN,zh;q=0.8");
    var xhrdata = "companyCode=###companyCode&mailNo=###mailNo&taobaoTradeId=###taobaoTradeId";
    xhr.send(xhrdata)
    if (xhr.status == 200)
        return xhr.responseText;
    return "ERROR:" + xhr.status;
})();
//TAOBAO_MARK_DELIVERY

//TAOBAO_MODIFY_COMMENT
(function() {
    var data = "false";
    var xhr = new XMLHttpRequest();
    var url = "https://trade.tmall.com/detail/orderDetail.htm?bizOrderId={0}";

    xhr.open("GET", url, false);

    xhr.setRequestHeader("accept", "application/json, text/javascript, */*; q=0.01");
    xhr.setRequestHeader("accept-encoding", "gzip, deflate, br");
    xhr.setRequestHeader("accept-language", "zh-CN,zh;q=0.8");
    xhr.setRequestHeader("content-type", "application/x-www-form-urlencoded; charset=UTF-8");

    xhr.send()

    if (xhr.status == 200)
        return xhr.responseText;
    return "ERROR:" + xhr.status;
})();
//TAOBAO_MODIFY_COMMENT


//TMALL_QUERY_RATE
(function() {
    var data = "false";
    var xhr = new XMLHttpRequest();
    var url ="https://rate.tmall.com/list_detail_rate.htm?itemId=###itemId&sellerId=###sellerId&order=1&currentPage=###currentPage&append=0&content=0&tagId=&posi=&picture=0";

    xhr.open("GET", url, false);

    //xhr.setRequestHeader("accept", "*/*");
    //xhr.setRequestHeader("accept-encoding", "gzip, deflate, br");
    //xhr.setRequestHeader("accept-language", "zh-CN,zh;q=0.8");

    xhr.send()

    if (xhr.status == 200)
        return xhr.responseText;
    return "ERROR:" + xhr.status;
})();
//TMALL_QUERY_RATE